using System.Reflection;

namespace ManosabaLin.Compat.Hextech;

/// <summary>
///     海克斯符文（HextechRunes）**软依赖**门控。
///     <para>
///         依据海克斯作者发布的《外部模组对接指南》：
///         <list type="bullet">
///             <item>按 <b>程序集名</b> <c>HextechRunes</c> 检测（<b>不是</b> manifest id —— 对方按游戏版本加载不同变体 dll，
///                 二创版也可能用同一程序集名）。</item>
///             <item>模组之间初始化顺序不固定 ⇒ 先查一次已加载程序集，没找到就订阅
///                 <see cref="AppDomain.AssemblyLoad" />，载入后立即注册。</item>
///             <item>注册必须在<b>模组初始化阶段</b>完成（要赶在共享遗物池首次枚举之前）。</item>
///         </list>
///     </para>
///     <para>
///         ⚠️ 本文件<b>刻意不引用</b> <c>HextechRunes.dll</c>：全程字符串 + 反射 ⇒ 对方没装也能正常加载本 mod。
///         因此产物 DLL 里不会出现对 <c>HextechRunes</c> 的任何编译期引用。
///     </para>
///     <para>
///         ⚠️ <b>绝不对对方的 <c>internal</c> 类（如 <c>HextechCatalog</c>）打 Harmony 补丁</b> —— 指南明令禁止。
///         缺能力就开 issue（草稿见 <c>docs/hextech-compat-hextech.md</c>）。
///     </para>
/// </summary>
internal static class HextechCompat
{
    /// <summary>按<b>程序集名</b>检测，不是 manifest id。</summary>
    public const string AssemblyName = "HextechRunes";

    /// <summary>软依赖入口类型全名。</summary>
    public const string InteropTypeName = "HextechRunes.HextechRunesInterop";

    /// <summary>软依赖公开面的最低版本；低于它整体跳过。</summary>
    public const int MinimumApiVersion = 1;

    /// <summary>我方日志前缀（与海克斯侧的 <c>[HextechRunes][ExternalContent]</c> 对得上）。</summary>
    public const string LogPrefix = "[ManosabaLin][Hextech]";

    private static readonly object Gate = new();

    private static bool _listening;
    private static bool _bindAttempted;

    /// <summary>已绑定的海克斯程序集；未装为 <c>null</c>。</summary>
    public static System.Reflection.Assembly? TargetAssembly { get; private set; }

    /// <summary>已绑定的 <c>HextechRunesInterop</c> 类型；解析失败为 <c>null</c>。</summary>
    public static Type? InteropType { get; private set; }

    /// <summary>对方的 <c>ApiVersion</c>；<c>0</c> = 不可用（太旧 / 取不到）。</summary>
    public static int ApiVersion { get; private set; }

    /// <summary>是否已具备可用的软依赖面。</summary>
    public static bool IsReady =>
        TargetAssembly is not null && InteropType is not null && ApiVersion >= MinimumApiVersion;

    /// <summary>
    ///     在 <c>MainFile.Initialize()</c> 里调用一次。
    ///     已加载 ⇒ 立即绑定并注册；未加载 ⇒ 订阅 <see cref="AppDomain.AssemblyLoad" /> 等它。
    /// </summary>
    public static void Initialize()
    {
        try
        {
            var loaded = FindLoadedAssembly();
            if (loaded is not null)
            {
                Bind(loaded);
                return;
            }

            lock (Gate)
            {
                if (_listening || _bindAttempted) return;
                _listening = true;
                AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
            }

            Info($"assembly '{AssemblyName}' not loaded yet; waiting for AssemblyLoad.");
        }
        catch (Exception ex)
        {
            Warn($"Initialize failed: {ex.Message}");
        }
    }

    private static System.Reflection.Assembly? FindLoadedAssembly()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (string.Equals(assembly.GetName().Name, AssemblyName, StringComparison.Ordinal))
                return assembly;
        }

        return null;
    }

    private static void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs e)
    {
        try
        {
            if (!string.Equals(e.LoadedAssembly.GetName().Name, AssemblyName, StringComparison.Ordinal))
                return;

            lock (Gate)
            {
                AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
                _listening = false;
            }

            Info("assembly loaded late; registering now.");
            Bind(e.LoadedAssembly);
        }
        catch (Exception ex)
        {
            Warn($"AssemblyLoad handler failed: {ex.Message}");
        }
    }

    /// <summary>绑定程序集 + 读 <c>ApiVersion</c>；只在拿到可用版本后才去注册。</summary>
    private static void Bind(System.Reflection.Assembly assembly)
    {
        Type? interopType;

        lock (Gate)
        {
            if (_bindAttempted) return;
            _bindAttempted = true;

            TargetAssembly = assembly;
            ApiVersion = 0;

            interopType = assembly.GetType(InteropTypeName, throwOnError: false);
            InteropType = interopType;

            if (interopType is null)
            {
                Warn($"type '{InteropTypeName}' not found —— 软依赖面不可用，整体跳过。");
                return;
            }

            // 能力探测优先于版本号：ApiVersion 只当「太旧就整体跳过」的门槛。
            var versionProperty = interopType.GetProperty("ApiVersion", BindingFlags.Public | BindingFlags.Static);
            if (versionProperty is null || versionProperty.PropertyType != typeof(int))
            {
                Info($"'{InteropTypeName}.ApiVersion' missing ⇒ 对方版本早于 0.9.6，整体跳过。");
                return;
            }

            ApiVersion = versionProperty.GetValue(null) is int version ? version : 0;
            if (ApiVersion < MinimumApiVersion)
                Info($"ApiVersion={ApiVersion} < {MinimumApiVersion} ⇒ 整体跳过。");
        }

        if (interopType is null || ApiVersion < MinimumApiVersion) return;

        Info($"bound HextechRunes (ApiVersion={ApiVersion}).");
        HextechRuneRegistrar.RegisterAll();
    }

    /// <summary>给 <see cref="HextechRuneRegistrar" /> 用的反射入口；<see cref="IsReady" /> 为 false 时返回 null。</summary>
    public static MethodInfo? ResolveInteropMethod(string name, Type[] parameterTypes)
    {
        if (!IsReady) return null;

        try
        {
            return InteropType!.GetMethod(
                name,
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: parameterTypes,
                modifiers: null);
        }
        catch (Exception ex)
        {
            Warn($"ResolveInteropMethod('{name}') failed: {ex.Message}");
            return null;
        }
    }

    public static void Info(string message) => MainFile.Logger.Info($"{LogPrefix} {message}");

    public static void Warn(string message) => MainFile.Logger.Warn($"{LogPrefix} {message}");
}
