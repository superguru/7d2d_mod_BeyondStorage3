using System;
using BeyondStorage.Infrastructure;

namespace BeyondStorage.Source.Configuration.Gears;

internal static class GearsConversions
{
    internal static bool ToBool(string value)
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
            return true;
        }
    }
}