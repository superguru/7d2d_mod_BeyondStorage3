using System;

namespace BeyondStorage.Infrastructure;

/// <summary>
/// Handles config schema versioning.
/// </summary>
public static class ConfigVersioning
{
    /// <summary>
    /// Current config schema version - always matches ModInfo.Version (lazy loaded)
    /// </summary>
    public static string CurrentVersion
    {
        get
        {
            if (string.IsNullOrEmpty(field))
            {
                field = ModInfo.Version;
            }
            return field;
        }
    } = null;

    /// <summary>
    /// Determines whether <paramref name="version"/> is older than <paramref name="compareTo"/>.
    /// Unparseable or missing versions are treated as older.
    /// </summary>
    /// <param name="version">Version string to check</param>
    /// <param name="compareTo">Version string to compare against</param>
    /// <returns>True if <paramref name="version"/> is older than <paramref name="compareTo"/></returns>
    public static bool IsOlderThan(string version, string compareTo)
    {
        if (!TryParseVersion(version, out var versionObject) ||
            !TryParseVersion(compareTo, out var compareToObject))
        {
            return true;
        }

        return versionObject < compareToObject;
    }

    /// <summary>
    /// Attempts to parse a version string into a Version object
    /// </summary>
    private static bool TryParseVersion(string versionString, out Version version)
    {
        version = null;
        if (string.IsNullOrEmpty(versionString))
        {
            return false;
        }

        return Version.TryParse(versionString, out version);
    }
}
