using BeyondStorage.Configuration;
using GearsAPI.Settings.Global;

namespace BeyondStorage.Source.Configuration.Gears;

internal static class GearsGeneralSettings
{
    public static void SetDebugMode(IGlobalModSetting setting, string newValue)
    {
        var value = GearsConversions.ToBool(newValue);
        var oldValue = ModConfig.ClientConfig.isDebug;

        if (oldValue != value)
        {
            ModConfig.ClientConfig.isDebug = value;
            ModConfig.SaveConfig();
        }
    }
}
