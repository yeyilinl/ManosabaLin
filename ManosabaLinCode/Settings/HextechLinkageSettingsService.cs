using STS2RitsuLib;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;

namespace ManosabaLin.Settings;

/// <summary>
///     海克斯符文联动的总开关。
///     <para>
///         默认开启。关闭后，<c>Compat.Hextech.HextechRuneCatalog</c> 里 9 个符文的
///         <c>Availability</c> 谓词会整体返回 false ⇒ 它们不会进入海克斯的符文三选一池
///         （无论当前角色是否为安安/希罗/雪莉）。
///     </para>
///     <para>
///         ⚠️ 该谓词会在<b>每层选海克斯时</b>被海克斯实时调用（每玩家各自评估自己的本地设置），
///         因此本开关<b>即时生效、无需重启</b>。联机两端默认都为开 ⇒ 结果一致；
///         若某位联机玩家单独关闭，仅其本机不再出现联动符文（视觉差异，不进入权威性状态）。
///     </para>
/// </summary>
public static class HextechLinkageSettingsService
{
    private const string SettingsLocTable = "settings_ui";
    private const string SettingsDataKey = "hextech_linkage_settings";
    private const string SettingsFileName = "hextech_linkage_settings.json";

    private static bool registeredData;
    private static bool settingsCacheLoaded;
    private static HextechLinkageSettings cachedSettings = new();

    public static readonly IModSettingsValueBinding<bool> EnabledBinding =
        ModSettingsBindings.WithDefault(
            ModSettingsBindings.Callback(
                MainFile.ModId,
                $"{SettingsDataKey}.enabled",
                () => ReadSettings().Enabled,
                value => UpdateSettings(settings => settings.Enabled = value),
                SaveSettings),
            () => true);

    public static bool IsEnabled => EnabledBinding.Read();

    public static void RegisterSettingsData()
    {
        if (registeredData) return;

        using (RitsuLibFramework.BeginModDataRegistration(MainFile.ModId))
        {
            RitsuLibFramework.GetDataStore(MainFile.ModId).Register(
                SettingsDataKey,
                SettingsFileName,
                SaveScope.Global,
                () => new HextechLinkageSettings(),
                autoCreateIfMissing: true);
        }

        registeredData = true;
        LoadSettingsCache();
    }

    public static ModSettingsText T(string key, string fallback)
        => ModSettingsText.LocString(SettingsLocTable, key, fallback);

    private static HextechLinkageSettings ReadSettings()
    {
        if (!settingsCacheLoaded) LoadSettingsCache();
        return cachedSettings;
    }

    private static void LoadSettingsCache()
    {
        try
        {
            cachedSettings = registeredData
                ? RitsuLibFramework.GetDataStore(MainFile.ModId).Get<HextechLinkageSettings>(SettingsDataKey)
                    ?? new HextechLinkageSettings()
                : cachedSettings;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[HextechLinkageSettings] Failed to load settings: {ex.Message}");
            cachedSettings = new HextechLinkageSettings();
        }

        settingsCacheLoaded = true;
    }

    private static void UpdateSettings(Action<HextechLinkageSettings> update)
    {
        var settings = ReadSettings();
        update(settings);
        cachedSettings = settings;

        if (!registeredData) return;

        try
        {
            RitsuLibFramework.GetDataStore(MainFile.ModId).Modify<HextechLinkageSettings>(
                SettingsDataKey,
                persisted => persisted.Enabled = cachedSettings.Enabled);
            SaveSettings();
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[HextechLinkageSettings] Failed to write settings: {ex.Message}");
        }
    }

    private static void SaveSettings()
    {
        if (!registeredData) return;

        try
        {
            RitsuLibFramework.GetDataStore(MainFile.ModId).Save(SettingsDataKey);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[HextechLinkageSettings] Failed to save settings: {ex.Message}");
        }
    }
}

public sealed class HextechLinkageSettings
{
    public bool Enabled { get; set; } = true;
}
