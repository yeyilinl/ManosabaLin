using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Players;

namespace ManosabaLin.Compat.Hextech;

/// <summary>
///     注册桥：把 <see cref="HextechRuneCatalog" /> 里的我方符文交给海克斯的**公开注册 API**。
///     <para>
///         <b>零 Harmony 补丁</b>（指南明令禁止 patch 对方的 <c>internal</c> 类），
///         全部通过反射调用 <c>HextechRunes.HextechRunesInterop</c> 的 public static 成员。
///     </para>
///     <para>
///         <b>能力探测优先于版本号</b>：每个成员单独用
///         <see cref="HextechCompat.ResolveInteropMethod" /> 解析，解析不到就<b>只降级那一步</b>，
///         不整体放弃。⚠️ 指南的签名可能与本机版本不一致（本机版本可能更旧）—— 这是有意的健壮性设计。
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         <b>逐符文幂等</b>：登记成功的符文记在 <see cref="RegisteredRunes" /> 里，重复调用
///         <see cref="RegisterAll" /> 只补「还没成功的那几个」。
///         ⇒ <see cref="HextechCompat" /> 的兜底重试（<c>GameReady</c> / <c>MainMenuReady</c>）
///         可以放心地再调一次，不会重复注册、也不会打断已成功的部分。
///     </para>
///     <para>
///         ⚠️ <b>注册失败必须吵</b>：逐符文 <c>try/catch</c> 是为了「绝不让单个符文打断整个 mod 的初始化」，
///         但代价是失败**不会崩**、只是那几张牌整局不出现（还完全没有别的症状）。
///         所以「不齐」时走 <c>Error</c> 级日志 + 逐条原因，不允许静默降级。
///     </para>
/// </remarks>
internal static class HextechRuneRegistrar
{
    // ⚠️ 参数类型列表必须与指南签名逐字一致，否则 GetMethod 返回 null（= 该能力不可用）。
    private static readonly Type[] RegisterPlayerRuneParameters =
    [
        typeof(Type), typeof(string), typeof(string), typeof(string), typeof(int),
        typeof(string), typeof(string), typeof(Func<Player, bool>)
    ];

    private static readonly Type[] SetPlayerRunePoolLabelParameters = [typeof(Type), typeof(string)];

    private static readonly Type[] RegisterConfigSectionTitleParameters = [typeof(string), typeof(string)];

    private static readonly object Gate = new();

    /// <summary>已经成功登记到海克斯注册表里的符文（按类型名）。</summary>
    private static readonly HashSet<string> RegisteredRunes = new(StringComparer.Ordinal);

    private static bool _auxiliaryDone;

    /// <summary>
    ///     目录里的符文是否**全部**登记成功。
    ///     <para>
    ///         给两层用：① <see cref="HextechCompat.TryBind" /> 判断「还需不需要再绑一次」；
    ///         ② <see cref="HextechCompat.SubscribeLateSweeps" /> 判断「还需不需要挂兜底重试」。
    ///     </para>
    /// </summary>
    public static bool AllRegistered
    {
        get
        {
            lock (Gate)
                return RegisteredRunes.Count >= HextechRuneCatalog.All.Count;
        }
    }

    /// <summary>执行一次注册（幂等：已成功的符文不会再注册，可安全重复调用）。</summary>
    public static void RegisterAll()
    {
        if (!HextechCompat.IsReady)
        {
            HextechCompat.Info("not ready ⇒ skip registration.");
            return;
        }

        // 目录自检放在最前面：把「配错了但编译得过」变成显式日志。
        var problems = HextechRuneCatalog.Validate();
        foreach (var problem in problems)
            HextechCompat.Warn($"catalog problem: {problem}");

        RegisterRunes();

        // 来源标签 / 配置菜单分组标题只对「已登记的符文」有意义；符文不齐时先做一次尽力而为，
        // 并把「已完成」标记留空 —— 等兜底重试把剩余符文补上后，这里会再做一次。
        if (!_auxiliaryDone)
        {
            RegisterPoolLabels();
            RegisterConfigSection();
            _auxiliaryDone = AllRegistered;
        }
    }

    private static void RegisterRunes()
    {
        var register = HextechCompat.ResolveInteropMethod("RegisterPlayerRune", RegisterPlayerRuneParameters);
        if (register is null)
        {
            HextechCompat.Error(
                "'RegisterPlayerRune' 解析失败（对方版本可能早于本指南）⇒ 9 个符文全部未注册。");
            return;
        }

        var newlyRegistered = new List<string>();
        var failures = new List<(string Name, Exception Error)>();
        var pending = 0;

        foreach (var spec in HextechRuneCatalog.All)
        {
            if (IsRegistered(spec.RuneType.Name)) continue;

            pending++;

            object?[] args =
            [
                spec.RuneType,
                spec.Rarity,
                spec.Flags,
                spec.CharacterPool,
                spec.CharacterOrder,
                spec.TagKey,
                spec.AssetModId,
                spec.Availability
            ];

            try
            {
                register.Invoke(null, args);
                MarkRegistered(spec.RuneType.Name);
                newlyRegistered.Add(spec.RuneType.Name);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                // 参数无效 ⇒ 对方抛 ArgumentException，被反射包成 TargetInvocationException。
                // ⚠️ 必须捕获：绝不能让单个符文打断整个 mod 的初始化。
                failures.Add((spec.RuneType.Name, ex.InnerException));
            }
            catch (Exception ex)
            {
                failures.Add((spec.RuneType.Name, ex));
            }
        }

        // 本次没有任何待办（都注册过了）⇒ 不必重复打日志。
        if (pending == 0) return;

        var total = RegisteredCount;
        var count = HextechRuneCatalog.All.Count;

        if (total >= count)
        {
            HextechCompat.Info(
                $"RegisterPlayerRune: ok={total}/{count} [{string.Join(", ", RegisteredNames())}]" +
                (newlyRegistered.Count > 0 ? $" (+{newlyRegistered.Count} this call)" : string.Empty));
            return;
        }

        // ⚠️ 这里**不能**只是 Warn：少注册 = 那几张牌整局不出现，且没有任何其它症状。
        // 用 Error 级 + 逐条原因，把「静默少注册」变成一眼可见（唯一可靠的验收手段就是看这行）。
        HextechCompat.Error(
            $"RegisterPlayerRune: ok={total}/{count} —— {count - total} 个符文**未注册**，" +
            "它们在本局里永远不会出现在海克斯的符文三选一里。" +
            "若原因是注册窗口已关闭（异常里提到 pool 或 ModelIdSerializationCache），" +
            "说明海克斯比本模组晚加载到了窗口之外。");

        foreach (var (name, error) in failures)
            HextechCompat.Error($"RegisterPlayerRune failed: {name}({error.GetType().Name}: {error.Message})");
    }

    /// <summary>三选一界面上的「来源标签」；文案取 <c>relic_collection</c> 表的 <c>HEXTECH_POOL.&lt;poolKey&gt;</c>。</summary>
    private static void RegisterPoolLabels()
    {
        var setLabel = HextechCompat.ResolveInteropMethod("SetPlayerRunePoolLabel", SetPlayerRunePoolLabelParameters);
        if (setLabel is null)
        {
            HextechCompat.Info("'SetPlayerRunePoolLabel' 不可用 ⇒ 来源标签保持默认。");
            return;
        }

        var count = 0;

        foreach (var spec in HextechRuneCatalog.All)
        {
            try
            {
                setLabel.Invoke(null, [spec.RuneType, HextechRuneCatalog.PoolLabelKey]);
                count++;
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                HextechCompat.Warn(
                    $"SetPlayerRunePoolLabel('{spec.RuneType.Name}') failed: {ex.InnerException.Message}");
            }
            catch (Exception ex)
            {
                HextechCompat.Warn($"SetPlayerRunePoolLabel('{spec.RuneType.Name}') failed: {ex.Message}");
            }
        }

        HextechCompat.Info($"SetPlayerRunePoolLabel: ok={count}/{HextechRuneCatalog.All.Count}");
    }

    /// <summary>配置菜单里我方这组内容的小标题；文案键是 <c>relic_collection</c> 表的完整键本身。</summary>
    private static void RegisterConfigSection()
    {
        var registerTitle = HextechCompat.ResolveInteropMethod(
            "RegisterConfigSectionTitle",
            RegisterConfigSectionTitleParameters);

        if (registerTitle is null)
        {
            HextechCompat.Info("'RegisterConfigSectionTitle' 不可用 ⇒ 配置菜单分组标题保持默认。");
            return;
        }

        try
        {
            registerTitle.Invoke(null, [HextechRuneCatalog.AssetModId, HextechRuneCatalog.ConfigSectionTitleKey]);
            HextechCompat.Info($"RegisterConfigSectionTitle: ok ('{HextechRuneCatalog.ConfigSectionTitleKey}')");
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            HextechCompat.Warn($"RegisterConfigSectionTitle failed: {ex.InnerException.Message}");
        }
        catch (Exception ex)
        {
            HextechCompat.Warn($"RegisterConfigSectionTitle failed: {ex.Message}");
        }
    }

    /// <summary>测试/排障用：把已解析到的软依赖成员列出来。</summary>
    public static IReadOnlyList<string> DescribeCapabilities()
    {
        var result = new List<string> { $"ApiVersion={HextechCompat.ApiVersion}" };

        void Probe(string name, Type[] parameters)
        {
            var method = HextechCompat.ResolveInteropMethod(name, parameters);
            result.Add($"{name}: {(method is null ? "unavailable" : "ok")}");
        }

        Probe("RegisterPlayerRune", RegisterPlayerRuneParameters);
        Probe("SetPlayerRunePoolLabel", SetPlayerRunePoolLabelParameters);
        Probe("RegisterConfigSectionTitle", RegisterConfigSectionTitleParameters);
        result.Add($"AllRegistered={AllRegistered}");

        return result;
    }

    private static bool IsRegistered(string runeTypeName)
    {
        lock (Gate)
            return RegisteredRunes.Contains(runeTypeName);
    }

    private static void MarkRegistered(string runeTypeName)
    {
        lock (Gate)
            RegisteredRunes.Add(runeTypeName);
    }

    private static int RegisteredCount
    {
        get
        {
            lock (Gate)
                return RegisteredRunes.Count;
        }
    }

    private static IReadOnlyList<string> RegisteredNames()
    {
        lock (Gate)
            return [.. RegisteredRunes];
    }
}
