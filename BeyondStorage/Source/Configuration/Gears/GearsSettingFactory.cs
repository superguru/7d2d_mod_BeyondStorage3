using System;

namespace BeyondStorage.Configuration.Gears;

internal static class GearsSettingFactory
{
    // The switches are declared as type="string" in ModSettings.xml so the menu can label them Off/On.
    internal static GearsSetting<bool, string> Bool(string key, Func<ModConfigData, bool> getConfig, Action<ModConfigData, bool> setConfig)
    {
        return new GearsSetting<bool, string>(
            key,
            getConfig,
            setConfig,
            value => GearsConversions.ToBool(value, false),
            GearsConversions.FromBool);
    }

    internal static GearsSetting<int, int> Int(string key, Func<ModConfigData, int> getConfig, Action<ModConfigData, int> setConfig)
    {
        return new GearsSetting<int, int>(
            key,
            getConfig,
            setConfig,
            value => value,
            value => value);
    }

    internal static GearsSetting<float, float> Float(string key, Func<ModConfigData, float> getConfig, Action<ModConfigData, float> setConfig)
    {
        return new GearsSetting<float, float>(
            key,
            getConfig,
            setConfig,
            value => value,
            value => value);
    }
}