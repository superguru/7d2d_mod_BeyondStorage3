using BeyondStorage.Infrastructure;
using GearsAPI.Settings;
using GearsAPI.Settings.Global;
using GearsAPI.Settings.World;

namespace BeyondStorage.Source.Configuration.Gears;

public class GearsMod_Init : IGearsModApi
{
    private static IGearsMod s_gearsMod;
    private static IModGlobalSettings s_GearsGlobalSettings;


    void IGearsModApi.InitMod(IGearsMod modInstance)
    {
        s_gearsMod = modInstance;
    }

    void IGearsModApi.OnGlobalSettingsLoaded(IModGlobalSettings modSettings)
    {
        s_GearsGlobalSettings = modSettings;
        if (s_GearsGlobalSettings == null)
        {
            ModLogger.DebugLog($"Global settings loaded, but modSettings is null, and gears mod is {s_gearsMod}");
            return;
        }

        var generalTab = s_GearsGlobalSettings.GetTab("General");
        if (generalTab == null)
        {
            ModLogger.DebugLog($"Global settings loaded, but generalTab is null");
            return;
        }

        var generalCategory = generalTab.GetCategory("General");
        if (generalTab == null)
        {
            ModLogger.DebugLog($"Global settings loaded, but generalCategory is null");
            return;
        }

        var isDebugSetting = (generalCategory.GetSetting("IsDebug") as IGlobalValueSetting);
        if (isDebugSetting == null)
        {
            ModLogger.DebugLog($"Global settings loaded, but isDebugSetting is null");
            return;
        }

        isDebugSetting.OnSettingChanged += GearsGeneralSettings.SetDebugMode;
        GearsGeneralSettings.SetDebugMode(isDebugSetting, isDebugSetting.CurrentValue);
    }

    void IGearsModApi.OnWorldSettingsLoaded(IModWorldSettings worldSettings)
    {
        // NOP
    }
}
