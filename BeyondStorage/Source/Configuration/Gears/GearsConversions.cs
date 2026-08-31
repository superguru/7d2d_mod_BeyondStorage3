using System;
using ZoneControl.Infrastructure;

namespace ZoneControl.Configuration.Gears;

internal static class GearsConversions
{
    public static bool IsEqualValue(string value, bool b)
    {
        var a = ToBool(value, !b);
        return a == b;
    }

    public static bool IsEqualValue(string value, int b)
    {
        // ~b is a fallback that can never equal b, so a failed parse reads as "not equal"
        var a = ToInt(value, ~b);
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

    internal static string FromInt(int value, int min, int max)
    {
        if (value < min)
        {
            value = min;
        }
        else if (value > max)
        {
            value = max;
        }

        return value.ToString();
    }

    internal static string FromFloat(float value, float min, float max)
    {
        if (value < min)
        {
            value = min;
        }
        else if (value > max)
        {
            value = max;
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
    internal static int ToInt(string value, int defaultValue)
    {
        if (!int.TryParse(value, out var convertedValue))
        {
            convertedValue = defaultValue;
        }

        return convertedValue;
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