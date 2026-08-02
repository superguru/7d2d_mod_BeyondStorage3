using BeyondStorage.Infrastructure;
using GearsAPI.Settings;
using GearsAPI.Settings.Global;
using GearsAPI.Settings.World;

namespace BeyondStorage.Source.Configuration.Gears;

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

        var generalTab = GearsGlobalSettings.GetTab("General");
        if (generalTab == null)
        {
            ModLogger.DebugLog($"Global settings loaded, but generalTab is null");
            return;
        }

        var generalCategory = generalTab.GetCategory("General");
        if (generalCategory == null)
        {
            ModLogger.DebugLog($"Global settings loaded, but generalCategory is null");
            return;
        }

        GearsGeneralSettings.Configure(generalCategory);
    }

    void IGearsModApi.OnWorldSettingsLoaded(IModWorldSettings worldSettings)
    {
        // NOP
    }
}
