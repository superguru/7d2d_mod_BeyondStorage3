using BeyondStorage.Infrastructure;
using GearsAPI.Settings;
using GearsAPI.Settings.Global;
using GearsAPI.Settings.World;

namespace BeyondStorage.Configuration.Gears;

public class GearsModAPI : IGearsModApi
{
    private static IGearsMod s_gearsMod;

    private static IModGlobalSettings GearsGlobalSettings
    {
        get; set;
    }

    void IGearsModApi.InitMod(IGearsMod modInstance)
    {
        s_gearsMod = modInstance;
    }

    public static void SaveGlobalSettings()
    {
        GearsGlobalSettings?.SaveSettings();
    }

    void IGearsModApi.OnGlobalSettingsLoaded(IModGlobalSettings modSettings)
    {
        GearsGlobalSettings = modSettings;
        if (GearsGlobalSettings == null)
        {
            ModLogger.DebugLog($"Global settings loaded, but modSettings is null, and gears mod is {s_gearsMod}");
            return;
        }

        foreach (var (tabName, categoryName, settings) in GearsSettingsRegistry.Entries)
        {
            var tab = GearsGlobalSettings.GetTab(tabName);
            if (tab == null)
            {
                ModLogger.DebugLog($"Global settings loaded, but `{tabName}` tab is null");
                continue;
            }

            var category = tab.GetCategory(categoryName);
            if (category == null)
            {
                ModLogger.DebugLog($"Global settings loaded, but `{categoryName}` category is null");
                continue;
            }

            foreach (var setting in settings)
            {
                setting.Bind(category);
            }
        }
    }

    void IGearsModApi.OnWorldSettingsLoaded(IModWorldSettings worldSettings)
    {
        // NOP
    }
}
