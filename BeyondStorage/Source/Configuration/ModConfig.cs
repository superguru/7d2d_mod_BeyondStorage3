using System;
using System.IO;
using BeyondStorage.Infrastructure;
using BeyondStorage.Multiplayer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BeyondStorage.Configuration;

public static class ModConfig
{
    public const float DEFAULT_RANGE = MIN_RANGE;
    public const float MIN_RANGE = 0.0f;  // Whatever the game holds in memory
    public const float MAX_RANGE = 250.0f; // Largest user settable maximum range

    private const string ConfigFileName = "modconfig.json";
    private const string DefaultsFileName = "modconfig.defaults.json";
    private const string LegacyConfigFileName = "config.json";
    private const string ReplacedByConfigFileName = "config_json_replaced-by_modconfig_json.txt";
    private const string UserMetaDescription = "USER Configuration file for Beyond Storage mod package";

    /// <summary>
    /// Maximum allowed config file size in bytes (1KB) to prevent abuse
    /// </summary>
    private const long MaxConfigFileSize = 1024;

    public static ModConfigData ClientConfig
    {
        get; private set;
    }
    public static ModConfigData ServerConfig { get; } = new();
    private static bool IsConfigLoaded { get; set; } = false;

    /// <summary>
    /// Gets the full path to the user configuration file
    /// </summary>
    /// <returns>Full path to the modconfig.json file</returns>
    private static string GetConfigFilePath()
    {
        return Path.Combine(ModPathManager.GetConfigPath(true), ConfigFileName);
    }

    /// <summary>
    /// Gets the full path to the shipped defaults configuration file
    /// </summary>
    /// <returns>Full path to the modconfig.defaults.json file</returns>
    private static string GetDefaultsFilePath()
    {
        return Path.Combine(ModPathManager.GetConfigPath(true), DefaultsFileName);
    }

    public static void LoadConfig()
    {
        // Reset loaded state so reload calls correctly track state
        IsConfigLoaded = false;

        // Rename a pre-v3.2.0 "config.json" to "modconfig.json" so older users' values are preserved
        MigrateConfigFileName();

        var configPath = GetConfigFilePath();
        var defaultsPath = GetDefaultsFilePath();

        ModLogger.Info($"Loading config from {configPath}");

        if (!File.Exists(defaultsPath))
        {
            LoadWithoutDefaults(configPath);
            return;
        }

        var defaultsJson = ReadConfigFile(defaultsPath);
        if (defaultsJson == null)
        {
            ModLogger.Error($"Failed to read defaults config file {defaultsPath}. Falling back to built-in defaults.");
            LoadWithoutDefaults(configPath);
            return;
        }

        var userJson = File.Exists(configPath) ? ReadConfigFile(configPath) : null;

        if (userJson != null && !NeedsMerge(userJson, defaultsJson))
        {
            // User config is already at (or newer than) the shipped defaults; load it as-is.
            var loaded = SafeDeserializeConfig(userJson);
            if (loaded == null)
            {
                SetDefaultConfigAndMarkLoaded();
            }
            else
            {
                FinalizeConfigLoad(loaded);
            }
            return;
        }

        // Merge: fill missing attributes from the shipped defaults, keep the user's existing values.
        var merged = MergeConfigs(defaultsJson, userJson);
        if (merged == null)
        {
            SetDefaultConfigAndMarkLoaded();
            return;
        }

        merged.version = ConfigVersioning.CurrentVersion;
        SetUserMetaDescription(merged, defaultsJson);

        ClientConfig = merged;
        SaveConfig(configPath);
        ModLogger.Info($"Config merged and saved to version {ConfigVersioning.CurrentVersion}");

        FinalizeConfigLoad(merged);
    }

    /// <summary>
    /// Fallback path used when no shipped defaults file is present.
    /// </summary>
    private static void LoadWithoutDefaults(string configPath)
    {
        if (File.Exists(configPath))
        {
            LoadExistingConfig(configPath);
        }
        else
        {
            LoadDefaultConfig(configPath);
        }
    }

    /// <summary>
    /// Loads an existing user config file without any defaults merging.
    /// </summary>
    private static void LoadExistingConfig(string path)
    {
        try
        {
            if (!ValidateConfigFileSize(path))
            {
                SetDefaultConfigAndMarkLoaded();
                return;
            }

            var configJson = ReadConfigFile(path);
            if (configJson == null)
            {
                SetDefaultConfigAndMarkLoaded();
                return;
            }

            var loadedConfig = SafeDeserializeConfig(configJson);
            if (loadedConfig == null)
            {
                SetDefaultConfigAndMarkLoaded();
                return;
            }

            FinalizeConfigLoad(loadedConfig);
        }
        catch (Exception e)
        {
            ModLogger.Error($"Unexpected error loading config from {path}: {e.Message}. Using default config.", e);
            SetDefaultConfigAndMarkLoaded();
        }
    }

    /// <summary>
    /// Loads default configuration when no config file exists
    /// </summary>
    private static void LoadDefaultConfig(string path)
    {
        ModLogger.Warning($"Config file {path} not found, using default config.");
        SetDefaultConfigAndMarkLoaded();
        CreateDefaultConfigFile(path);
    }

    /// <summary>
    /// Creates and saves a default config file to the specified path
    /// </summary>
    private static void CreateDefaultConfigFile(string configPath)
    {
        try
        {
            SaveConfig(configPath);
            ModLogger.Info($"Created default config file at {configPath}");
        }
        catch (Exception ex)
        {
            ModLogger.Warning($"Failed to create default config file at {configPath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates that the config file size is within acceptable limits
    /// </summary>
    private static bool ValidateConfigFileSize(string path)
    {
        var fileInfo = new FileInfo(path);
        if (fileInfo.Length > MaxConfigFileSize)
        {
            ModLogger.Error($"Config file is too large ({fileInfo.Length} bytes, max {MaxConfigFileSize} bytes). Using default config to prevent abuse.");
            return false;
        }

        ModLogger.DebugLog($"Config file size: {fileInfo.Length} bytes (within {MaxConfigFileSize} byte limit)");
        return true;
    }

    /// <summary>
    /// Reads and validates config file content
    /// </summary>
    private static string ReadConfigFile(string path)
    {
        string configJson;
        using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
        using (var reader = new StreamReader(fileStream))
        {
            var buffer = new char[MaxConfigFileSize];
            var charsRead = reader.Read(buffer, 0, buffer.Length);

            if (charsRead == buffer.Length && !reader.EndOfStream)
            {
                ModLogger.Error($"Config file content exceeds {MaxConfigFileSize} bytes. Truncated and using default config to prevent abuse.");
                return null;
            }

            configJson = new string(buffer, 0, charsRead);
        }

        if (string.IsNullOrWhiteSpace(configJson))
        {
            ModLogger.Warning("Config file is empty or contains only whitespace. Using default config.");
            return null;
        }

        return configJson;
    }

    /// <summary>
    /// Determines whether the user config needs to be merged with the shipped defaults.
    /// </summary>
    private static bool NeedsMerge(string userJson, string defaultsJson)
    {
        var defaultsVersion = GetJsonProperty(defaultsJson, "version");
        if (defaultsVersion == null)
        {
            // Defaults has no parseable version; merge to be safe.
            return true;
        }

        var userVersion = GetJsonProperty(userJson, "version");
        if (userVersion == null)
        {
            // User config has no parseable version; treat as older and merge.
            return true;
        }

        return ConfigVersioning.IsOlderThan(userVersion, defaultsVersion);
    }

    /// <summary>
    /// Extracts a string property value from a config JSON document.
    /// </summary>
    private static string GetJsonProperty(string json, string propertyName)
    {
        try
        {
            var jsonObject = JObject.Parse(json);
            return jsonObject[propertyName]?.Value<string>();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Marks the config's metaDescription as the user's config, unless the user has a custom value.
    /// </summary>
    private static void SetUserMetaDescription(ModConfigData config, string defaultsJson)
    {
        var defaultsMetaDescription = GetJsonProperty(defaultsJson, "metaDescription");

        if (string.IsNullOrEmpty(config.metaDescription) ||
            string.Equals(config.metaDescription, defaultsMetaDescription, StringComparison.Ordinal))
        {
            config.metaDescription = UserMetaDescription;
        }
    }

    /// <summary>
    /// Merges the shipped defaults and the user's existing config, with user values taking precedence.
    /// Missing values are ultimately filled from the ModConfigData field defaults.
    /// </summary>
    private static ModConfigData MergeConfigs(string defaultsJson, string userJson)
    {
        var settings = CreateJsonSerializerSettings();

        var config = new ModConfigData();

        try
        {
            JsonConvert.PopulateObject(defaultsJson, config, settings);
        }
        catch (Exception e)
        {
            ModLogger.Error($"Failed to apply defaults config: {e.Message}", e);
            return null;
        }

        if (!string.IsNullOrEmpty(userJson))
        {
            try
            {
                JsonConvert.PopulateObject(userJson, config, settings);
            }
            catch (Exception e)
            {
                ModLogger.Error($"Failed to apply user config: {e.Message}", e);
                return null;
            }
        }

        return config;
    }

    /// <summary>
    /// Creates the shared JSON serializer settings used for reading config files.
    /// </summary>
    private static JsonSerializerSettings CreateJsonSerializerSettings()
    {
        return new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Error = (sender, args) =>
            {
                ModLogger.Warning($"JSON deserialization warning: {args.ErrorContext.Error.Message}");
                args.ErrorContext.Handled = true;
            }
        };
    }

    /// <summary>
    /// Safely deserialize config JSON with additional error handling
    /// </summary>
    private static ModConfigData SafeDeserializeConfig(string configJson)
    {
        try
        {
            var settings = CreateJsonSerializerSettings();
            return JsonConvert.DeserializeObject<ModConfigData>(configJson, settings);
        }
        catch (JsonException e)
        {
            ModLogger.Error($"JSON deserialization failed: {e.Message}", e);
            return null;
        }
        catch (Exception e)
        {
            ModLogger.Error($"Unexpected error during config deserialization: {e.Message}", e);
            return null;
        }
    }

    /// <summary>
    /// Finalizes config loading by setting ClientConfig and validating
    /// </summary>
    private static void FinalizeConfigLoad(ModConfigData loadedConfig)
    {
        ClientConfig = loadedConfig;
        IsConfigLoaded = true;
        ModLogger.DebugLog($"Successfully loaded config");
        ValidateConfig();
    }

    /// <summary>
    /// Sets default config and marks it as loaded
    /// </summary>
    private static void SetDefaultConfigAndMarkLoaded()
    {
        ClientConfig = new ModConfigData();
        IsConfigLoaded = true;
    }

    /// <summary>
    /// Saves the current config to the default config file location
    /// </summary>
    public static void SaveConfig()
    {
        ValidateConfig(saveAlways: true);
    }

    /// <summary>
    /// Saves the current config to file with size validation
    /// </summary>
    private static void SaveConfig(string path)
    {
        try
        {
            string configJson;

            var serializer = JsonSerializer.CreateDefault();

            using (var sw = new StringWriter())
            using (var writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                writer.IndentChar = ' ';
                writer.Indentation = 4;

                serializer.Serialize(writer, ClientConfig);

                configJson = sw.ToString();
            }

            // Validate serialized config size before writing
            var configBytes = System.Text.Encoding.UTF8.GetByteCount(configJson);
            if (configBytes > MaxConfigFileSize)
            {
                ModLogger.Error($"Generated config is too large ({configBytes} bytes, max {MaxConfigFileSize} bytes). Not saving to prevent abuse.");
                return;
            }

            File.WriteAllText(path, configJson);

#if DEBUG
            ModLogger.DebugLog($"Config saved successfully ({configBytes} bytes)");
#endif
        }
        catch (Exception e)
        {
            ModLogger.Warning($"Failed to save config to {path}: {e.Message}");
        }
    }

    /// <summary>
    /// Validates and corrects configuration values. Saves config if any changes are made.
    /// </summary>
    private static void ValidateConfig(bool saveAlways = false)
    {
        bool configChanged = false;

        // Track if any validation methods make changes
        configChanged |= ValidateRangeOption();

        // Save config if any changes were made during validation
        if (configChanged || saveAlways)
        {
            try
            {
                var configPath = GetConfigFilePath();
                SaveConfig(configPath);
                ModLogger.DebugLog("Validated config saved to config file.");
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Failed to save config after validation corrections: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Validates and corrects the range option.
    /// </summary>
    /// <returns>True if the config was modified, false otherwise</returns>
    private static bool ValidateRangeOption()
    {
        if (ClientConfig.range < MIN_RANGE)
        {
            ModLogger.Warning($"Invalid range value {ClientConfig.range} in config, resetting to unlimited range");
            ClientConfig.range = MIN_RANGE;
            return true; // Config was modified
        }

        if (ClientConfig.range > MAX_RANGE)
        {
            ModLogger.Warning($"Invalid range value {ClientConfig.range} in config, resetting to {MAX_RANGE} (max user range limit).");
            ClientConfig.range = MAX_RANGE;
            return true; // Config was modified
        }

        return false; // No changes made
    }

    public static float Range()
    {
        float serverValue = ServerConfig.range;
        float clientValue = ClientConfig.range;

        return ServerUtils.HasServerConfig ? serverValue : clientValue;
    }

    public static bool AllowPushToAlliedVehicles()
    {
        bool serverValue = ServerConfig.allowPushToAlliedVehicles;
        bool clientValue = ClientConfig.allowPushToAlliedVehicles;

        return ServerUtils.HasServerConfig ? serverValue : clientValue;
    }

    public static bool IncludeDrones()
    {
        bool serverValue = ServerConfig.includeDrones;
        bool clientValue = ClientConfig.includeDrones;

        return ServerUtils.HasServerConfig ? serverValue : clientValue;
    }

    public static bool IncludeVehicles()
    {
        bool serverValue = ServerConfig.includeVehicles;
        bool clientValue = ClientConfig.includeVehicles;

        return ServerUtils.HasServerConfig ? serverValue : clientValue;
    }

    public static bool IsDebug()
    {
        return IsConfigLoaded && ClientConfig.isDebug;
    }

    public static bool ShowUseables()
    {
        return IsConfigLoaded && ClientConfig.showUseables;
    }

    /// <summary>
    /// Renames a pre-v3.2.0 "config.json" to the current "modconfig.json" filename (v3.2.0+). If both
    /// files already exist, the old one is renamed aside so its values aren't lost.
    /// </summary>
    private static void MigrateConfigFileName()
    {
        try
        {
            var configDir = ModPathManager.GetConfigPath();
            var oldFile = Path.Combine(configDir, LegacyConfigFileName);
            var newFile = Path.Combine(configDir, ConfigFileName);

            if (!File.Exists(oldFile))
            {
                return;
            }

            if (!File.Exists(newFile))
            {
                File.Move(oldFile, newFile);
                ModLogger.Info($"Renamed config file from {LegacyConfigFileName} to {ConfigFileName}");
                return;
            }

            var replacedByFile = Path.Combine(configDir, ReplacedByConfigFileName);
            if (File.Exists(replacedByFile))
            {
                File.Delete(replacedByFile);
            }
            File.Move(oldFile, replacedByFile);
            ModLogger.Info($"Both {LegacyConfigFileName} and {ConfigFileName} existed. Renamed {LegacyConfigFileName} to {ReplacedByConfigFileName}.");
        }
        catch (Exception ex)
        {
            ModLogger.Warning($"Failed to rename config file from {LegacyConfigFileName}: {ex.Message}");
        }
    }
}
