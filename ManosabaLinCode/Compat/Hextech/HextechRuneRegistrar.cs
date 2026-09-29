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
    private static bool _done;

    /// <summary>执行一次注册（幂等：重复调用直接返回）。</summary>
    public static void RegisterAll()
    {
        lock (Gate)
        {
            if (_done) return;
            _done = true;
        }

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
        RegisterPoolLabels();
        RegisterConfigSection();
    }

    private static void RegisterRunes()
    {
        var register = HextechCompat.ResolveInteropMethod("RegisterPlayerRune", RegisterPlayerRuneParameters);
        if (register is null)
        {
            HextechCompat.Warn("'RegisterPlayerRune' 解析失败（对方版本可能早于本指南）⇒ 符文未注册。");
            return;
        }

        var registered = new List<string>();
        var failed = new List<string>();

        foreach (var spec in HextechRuneCatalog.All)
        {
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
                registered.Add(spec.RuneType.Name);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                // 参数无效 ⇒ 对方抛 ArgumentException，被反射包成 TargetInvocationException。
                // ⚠️ 必须捕获：绝不能让单个符文打断整个 mod 的初始化。
                failed.Add($"{spec.RuneType.Name}({ex.InnerException.GetType().Name}: {ex.InnerException.Message})");
            }
            catch (Exception ex)
            {
                failed.Add($"{spec.RuneType.Name}({ex.GetType().Name}: {ex.Message})");
            }
        }

        // 打一条汇总：注册了几个 / 失败哪几个（静默失败是最贵的坑）。
        HextechCompat.Info(
            $"RegisterPlayerRune: ok={registered.Count}/{HextechRuneCatalog.All.Count}" +
            (registered.Count > 0 ? $" [{string.Join(", ", registered)}]" : string.Empty));

        foreach (var failure in failed)
            HextechCompat.Warn($"RegisterPlayerRune failed: {failure}");
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

        return result;
    }
}
