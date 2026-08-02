using System;
using BeyondStorage.Configuration;
using BeyondStorage.Infrastructure;

namespace BeyondStorage.Source.Configuration.Gears;

internal static class GearsConversions
{
    public static bool IsEqualValue(string value, bool b)
    {
        var a = ToBool(value, !b);
        return a == b;
    }

    public static bool IsEqualValue(string value, float b)
    {
        var a = ToFloat(value, -b);
        return a == b;
    }

    internal static string FromBool(bool value)
    {
        return value ? "On" : "Off";
    }

    internal static string FromFloat(float value)
    {
        if (value < ModConfig.RANGE_UNLIMITED)
        {
            value = ModConfig.RANGE_UNLIMITED;
        }
        else if (value > ModConfig.RANGE_MAX_USER_LIMIT)
        {
            value = ModConfig.RANGE_MAX_USER_LIMIT;
        }

        return value.ToString("F1");
    }

    internal static bool ToBool(string value, bool defaultValue)
    {
        if (string.Equals(value, "Off", StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }
        else if (string.Equals(value, "On", StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        else
        {
            ModLogger.DebugLog($"Cannot convert `{value}` to a bool setting value");
            return defaultValue;
        }
    }
    internal static float ToFloat(string value, float defaultValue)
    {
        if (!float.TryParse(value, out var convertedValue))
        {
            convertedValue = defaultValue;
        }

        return convertedValue;
    }
}