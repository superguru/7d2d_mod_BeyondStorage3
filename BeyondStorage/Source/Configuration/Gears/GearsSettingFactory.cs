using System;

namespace BeyondStorage.Configuration.Gears;

internal static class GearsSettingFactory
{
    internal static GearsSetting<bool> Bool(string key, Func<ModConfigData, bool> getConfig, Action<ModConfigData, bool> setConfig)
    {
        return new GearsSetting<bool>(
            key,
            getConfig,
            setConfig,
            value => GearsConversions.ToBool(value, false),
            GearsConversions.FromBool);
    }

    internal static GearsSetting<int> Int(string key, Func<ModConfigData, int> getConfig, Action<ModConfigData, int> setConfig, int min, int max, int defaultValue)
    {
        return new GearsSetting<int>(
            key,
            getConfig,
            setConfig,
            value => GearsConversions.ToInt(value, defaultValue),
            value => GearsConversions.FromInt(value, min, max));
    }

    internal static GearsSetting<float> Float(string key, Func<ModConfigData, float> getConfig, Action<ModConfigData, float> setConfig, float min, float max, float defaultValue)
    {
        return new GearsSetting<float>(
            key,
            getConfig,
            setConfig,
            value => GearsConversions.ToFloat(value, defaultValue),
            value => GearsConversions.FromFloat(value, min, max));
    }
}
