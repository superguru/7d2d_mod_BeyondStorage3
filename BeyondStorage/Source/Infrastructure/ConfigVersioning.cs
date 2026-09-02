using System;
using BeyondStorage.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BeyondStorage.Infrastructure;

/// <summary>
/// Handles config versioning, migration, and backwards compatibility
/// </summary>
public static class ConfigVersioning
{
    /// <summary>
    /// The first version to include versioning (2.3.0)
    /// </summary>
    public const string FirstVersionedConfig = "2.3.0";

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
    /// Detects if a config JSON string is legacy (pre-versioning)
    /// </summary>
    /// <param name="configJson">Raw JSON string from config file</param>
    /// <returns>True if legacy config (no version field)</returns>
    public static bool IsLegacyConfig(string configJson)
    {
        try
        {
            var jsonObject = JObject.Parse(configJson);
            return !jsonObject.ContainsKey("version");
        }
        catch (JsonException)
        {
            return true; // If we can't parse it, treat it as legacy
        }
    }

    /// <summary>
    /// Migrates a legacy config (pre-2.3.0) to the current versioned format
    /// </summary>
    /// <param name="legacyConfigJson">Legacy config JSON string</param>
    /// <returns>Migrated ModConfigData object</returns>
    public static ModConfigData MigrateLegacyConfig(string legacyConfigJson)
    {
        const string d_MethodName = nameof(MigrateLegacyConfig);
        ModLogger.Info($"{d_MethodName}: Migrating legacy config (pre-{FirstVersionedConfig}) to version {CurrentVersion}");

        try
        {
            // Parse legacy config without version field
            var legacyConfig = JsonConvert.DeserializeObject<LegacyBsConfig>(legacyConfigJson);

            // Create new versioned config with migrated values
            var migratedConfig = new ModConfigData
            {
                version = CurrentVersion,
                range = legacyConfig.range,
                includeDrones = legacyConfig.pullFromDrones,
                // pullFromCollectors removed in 2.6.9
                // pullFromWorkstationOutputs removed in 2.6.9
                includeVehicles = legacyConfig.pullFromVehicleStorage,
                // serverSyncConfig removed in 3.1.1
                isDebug = legacyConfig.isDebug
                // isDebugLogSettingsAccess removed in 2.6.7
            };

            ModLogger.Info($"{d_MethodName}: Successfully migrated legacy config");
            return migratedConfig;
        }
        catch (Exception e)
        {
            ModLogger.Error($"{d_MethodName}: Failed to migrate legacy config: {e.Message}", e);
            throw;
        }
    }

    /// <summary>
    /// Migrates a versioned config to the current version if needed
    /// </summary>
    /// <param name="config">Config to potentially migrate</param>
    /// <returns>Migrated config (may be the same object if no migration needed)</returns>
    public static ModConfigData MigrateVersionedConfig(ModConfigData config)
    {
        const string d_MethodName = nameof(MigrateVersionedConfig);

        if (config.version == CurrentVersion)
        {
            ModLogger.DebugLog($"{d_MethodName}: Config is already current version {CurrentVersion}");
            return config;
        }

        ModLogger.Info($"{d_MethodName}: Migrating config from version {config.version} to {CurrentVersion}");

        if (!TryParseVersion(config.version, out var fromVersion) ||
            !TryParseVersion(CurrentVersion, out var toVersion))
        {
            ModLogger.Warning($"{d_MethodName}: Unable to parse versions, using config as-is");
            config.version = CurrentVersion;
            return config;
        }

        var migratedConfig = config;

        // Migration to version 2.3.5: Disable debug mode on servers
        if (fromVersion < new Version("2.3.5"))
        {
            migratedConfig = MigrateTo235(migratedConfig);
        }

        // Migration to version 2.6.3: Remove pullFromPlayerCraftedNonCrates setting
        if (fromVersion < new Version("2.6.3"))
        {
            migratedConfig = MigrateTo263(migratedConfig);
        }

        // Migration to version 2.6.7: Remove isDebugLogSettingsAccess setting
        if (fromVersion < new Version("2.6.7"))
        {
            migratedConfig = MigrateTo267(migratedConfig);
        }

        // Migration to version 2.6.9: Remove pullFromCollectors and pullFromWorkstationOutputs settings
        if (fromVersion < new Version("2.6.9"))
        {
            migratedConfig = MigrateTo269(migratedConfig);
        }

        // Migration to version 3.1.1: Remove serverSyncConfig setting
        if (fromVersion < new Version("3.1.1"))
        {
            migratedConfig = MigrateTo311(migratedConfig);
        }

        // Migration to version 3.1.4: Add AllowPushToAlliedVehicles
        if (fromVersion < new Version("3.1.4"))
        {
            migratedConfig = MigrateTo314(migratedConfig);
        }

        migratedConfig.version = CurrentVersion;

        ModLogger.Info($"{d_MethodName}: Successfully migrated config to version {CurrentVersion}");
        return migratedConfig;
    }

    /// <summary>
    /// Attempts to parse a version string into a Version object
    /// </summary>
    private static bool TryParseVersion(string versionString, out System.Version version)
    {
        version = null;
        if (string.IsNullOrEmpty(versionString))
        {
            return false;
        }

        return System.Version.TryParse(versionString, out version);
    }

    /// <summary>
    /// Migrates config to version 2.3.5
    /// Changes: Disables debug mode when running on a server to prevent performance issues
    /// </summary>
    /// <param name="config">Config to migrate</param>
    /// <returns>Migrated config</returns>
    private static ModConfigData MigrateTo235(ModConfigData config)
    {
        const string d_MethodName = nameof(MigrateTo235);
        ModLogger.Info($"{d_MethodName}: Applying migration to version 2.3.5");

        if (config.isDebug && WorldTools.IsServer())
        {
            ModLogger.Info($"{d_MethodName}: Server environment detected - disabling debug mode for performance optimization");
            config.isDebug = false;
        }
        else if (config.isDebug)
        {
            ModLogger.Info($"{d_MethodName}: Debug mode remains enabled (not running on server)");
        }
        else
        {
            ModLogger.DebugLog($"{d_MethodName}: Debug mode already disabled - no changes needed");
        }

        ModLogger.Info($"{d_MethodName}: Successfully applied 2.3.5 migration");
        return config;
    }

    /// <summary>
    /// Migrates config to version 2.6.3
    /// Changes: Removes the pullFromPlayerCraftedNonCrates setting
    /// </summary>
    /// <param name="config">Config to migrate</param>
    /// <returns>Migrated config</returns>
    private static ModConfigData MigrateTo263(ModConfigData config)
    {
        const string d_MethodName = nameof(MigrateTo263);
        ModLogger.Info($"{d_MethodName}: Applying migration to version 2.6.3");
        ModLogger.Info($"{d_MethodName}: 'pullFromPlayerCraftedNonCrates' has been removed");
        ModLogger.Info($"{d_MethodName}: 'pullFromDewCollectors' has been renamed to 'pullFromCollectors'");
        return config;
    }

    /// <summary>
    /// Migrates config to version 2.6.7
    /// Changes: Removes the isDebugLogSettingsAccess setting — settings access logging
    /// is no longer configurable and will not be logged regardless of previous value.
    /// </summary>
    private static ModConfigData MigrateTo267(ModConfigData config)
    {
        const string d_MethodName = nameof(MigrateTo267);
        ModLogger.Info($"{d_MethodName}: Applying migration to version 2.6.7");
        ModLogger.Info($"{d_MethodName}: 'isDebugLogSettingsAccess' has been removed");
        return config;
    }

    /// <summary>
    /// Migrates config to version 2.6.9
    /// Changes: Removes pullFromCollectors and pullFromWorkstationOutputs — both are now always enabled.
    /// </summary>
    private static ModConfigData MigrateTo269(ModConfigData config)
    {
        const string d_MethodName = nameof(MigrateTo269);
        ModLogger.Info($"{d_MethodName}: Applying migration to version 2.6.9");
        ModLogger.Info($"{d_MethodName}: 'pullFromCollectors' has been removed — collectors are now always included");
        ModLogger.Info($"{d_MethodName}: 'pullFromWorkstationOutputs' has been removed — workstation outputs are now always included");
        return config;
    }

    /// <summary>
    /// Migrates config to version 3.1.1
    /// Changes: Removes the serverSyncConfig setting — server config sync is now always enabled.
    /// </summary>
    private static ModConfigData MigrateTo311(ModConfigData config)
    {
        const string d_MethodName = nameof(MigrateTo311);
        ModLogger.Info($"{d_MethodName}: Applying migration to version 3.1.1");
        ModLogger.Info($"{d_MethodName}: 'serverSyncConfig' has been removed — server config sync is now always enabled");
        return config;
    }

    /// <summary>
    /// Migrates config to version 3.1.4
    /// Changes: Add AllowPushToAlliedVehicles, which defaults to true
    /// </summary>
    private static ModConfigData MigrateTo314(ModConfigData config)
    {
        const string d_MethodName = nameof(MigrateTo314);

        ModLogger.Info($"{d_MethodName}: Applying migration to version 3.1.4");

        config.allowPushToAlliedVehicles = true;
        ModLogger.Info($"{d_MethodName}: 'allowPushToAlliedVehicles' has been added — default is true");

        return config;
    }

    /// <summary>
    /// Pre-processes raw config JSON to apply field renames before deserialization.
    /// Must be called before deserializing into ModConfigData to preserve renamed field values.
    /// </summary>
    /// <param name="configJson">Raw JSON string from config file</param>
    /// <returns>JSON string with field names updated to the current schema</returns>
    public static string PreprocessConfigJson(string configJson)
    {
        try
        {
            var jsonObject = JObject.Parse(configJson);

            if (!TryParseVersion(jsonObject["version"]?.Value<string>(), out var version))
            {
                return configJson;
            }

            RenamePullFromDewCollectors(jsonObject, version);
            RemoveIsDebugLogSettingsAccess(jsonObject, version);
            RemoveCollectorAndWorkstationFields(jsonObject, version);
            RemoveServerSyncConfig(jsonObject, version);
            RenameConsumeFieldsToInclude(jsonObject, version);
            RenameIncludeFromVehicles(jsonObject, version);

            return jsonObject.ToString(Formatting.None);
        }
        catch (JsonException)
        {
            return configJson;
        }
    }

    private static void RenamePullFromDewCollectors(JObject jsonObject, Version version)
    {
        // Pre-2.6.3: rename pullFromDewCollectors -> pullFromCollectors
        if (version < new Version("2.6.3") && jsonObject.ContainsKey("pullFromDewCollectors"))
        {
            jsonObject["pullFromCollectors"] = jsonObject["pullFromDewCollectors"];
            jsonObject.Remove("pullFromDewCollectors");
        }
    }

    private static void RemoveIsDebugLogSettingsAccess(JObject jsonObject, Version version)
    {
        // Pre-2.6.7: remove isDebugLogSettingsAccess
        if (version < new Version("2.6.7") && jsonObject.ContainsKey("isDebugLogSettingsAccess"))
        {
            jsonObject.Remove("isDebugLogSettingsAccess");
        }
    }

    private static void RemoveCollectorAndWorkstationFields(JObject jsonObject, Version version)
    {
        // Pre-2.6.9: remove pullFromCollectors and pullFromWorkstationOutputs
        if (version < new Version("2.6.9"))
        {
            jsonObject.Remove("pullFromCollectors");
            jsonObject.Remove("pullFromWorkstationOutputs");
        }
    }

    private static void RemoveServerSyncConfig(JObject jsonObject, Version version)
    {
        // Pre-3.1.1: remove serverSyncConfig
        if (version < new Version("3.1.1"))
        {
            jsonObject.Remove("serverSyncConfig");
        }
    }

    private static void RenameConsumeFieldsToInclude(JObject jsonObject, Version version)
    {
        // Pre-3.1.4: rename consumeFromDrones -> includeDrones, consumeFromVehicles -> includeVehicles.
        // Guard with ContainsKey so a pre-3.1.4 config that's missing the source keys (or has them
        // as null) doesn't write null into the new keys — leaving the keys absent lets the
        // deserializer fall back to the C# field default without logging a JSON warning.
        if (version < new Version("3.1.4"))
        {
            if (jsonObject.ContainsKey("consumeFromDrones"))
            {
                jsonObject["includeDrones"] = jsonObject["consumeFromDrones"];
            }
            if (jsonObject.ContainsKey("consumeFromVehicles"))
            {
                jsonObject["includeVehicles"] = jsonObject["consumeFromVehicles"];
            }
        }
    }

    private static void RenameIncludeFromVehicles(JObject jsonObject, Version version)
    {
        // Pre-3.1.8: rename incorrectly shipped includeFromVehicles -> includeVehicles
        if (version < new Version("3.1.8") && jsonObject.ContainsKey("includeFromVehicles"))
        {
            jsonObject["includeVehicles"] = jsonObject["includeFromVehicles"];
            jsonObject.Remove("includeFromVehicles");
        }
    }

    /// <summary>
    /// Legacy config structure for migration (pre-2.3.0)
    /// </summary>
    private class LegacyBsConfig
    {
        public float range = -1.0f;
        public bool pullFromDrones = true;
        public bool pullFromDewCollectors = true;
        public bool pullFromWorkstationOutputs = true;
        public bool pullFromVehicleStorage = true;
        public bool enableForBlockRepair = true;
        public bool enableForBlockTexture = true;
        public bool enableForBlockUpgrade = true;
        public bool enableForGeneratorRefuel = true;
        public bool enableForItemRepair = true;
        public bool enableForReload = true;
        public bool enableForVehicleRefuel = true;
        public bool enableForVehicleRepair = true;
        public bool serverSyncConfig = true;
        public bool consumeFromDrones = true;
        public bool consumeFromVehicles = true;
        public bool isDebug = false;
        // isDebugLogSettingsAccess intentionally omitted — removed in 2.6.7
    }
}