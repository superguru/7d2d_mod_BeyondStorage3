using BeyondStorage.Configuration;
using BeyondStorage.Infrastructure;
using GearsAPI.Settings.Global;

namespace BeyondStorage.Source.Configuration.Gears;

internal static class GearsGeneralSettings
{
    internal static void ConfigureGeneralCategorySettings(IGlobalModSettingsCategory generalCategory)
    {
        ConfigureRangeSetting(generalCategory);
        ConfigureAllowPushToAlliedVehiclesSetting(generalCategory);
        ConfigureIncludeDronesSetting(generalCategory);
        ConfigureIncludeVehiclesSetting(generalCategory);
        ConfigureIsDebugSetting(generalCategory);
    }

    private static bool TryGetGlobalValueSetting(IGlobalModSettingsCategory category, string key, out IGlobalValueSetting setting)
    {
        setting = (category.GetSetting(key) as IGlobalValueSetting);
        if (setting == null)
        {
            ModLogger.DebugLog($"Global settings loaded, but setting `{key}` is null");
            return false;
        }

        return true;
    }

    private static void ConfigureAllowPushToAlliedVehiclesSetting(IGlobalModSettingsCategory generalCategory)
    {
        if (!TryGetGlobalValueSetting(generalCategory, "AllowPushToAlliedVehicles", out IGlobalValueSetting setting))
        {
            return;
        }

        setting.OnSettingChanged += SetAllowPushToAlliedVehicles;
        SyncAllowPushToAlliedVehiclesSetting(setting);
    }

    private static void SetAllowPushToAlliedVehicles(IGlobalModSetting setting, string newValue)
    {
        var value = GearsConversions.ToBool(newValue, false);
        var oldValue = ModConfig.ClientConfig.allowPushToAlliedVehicles;

        if (oldValue != value)
        {
            ModConfig.ClientConfig.allowPushToAlliedVehicles = value;
            ModConfig.SaveConfig();
        }
    }
    private static void SyncAllowPushToAlliedVehiclesSetting(IGlobalValueSetting setting)
    {
        var modConfigValue = ModConfig.ClientConfig.allowPushToAlliedVehicles;

        if (!GearsConversions.IsEqualValue(setting.CurrentValue, modConfigValue))
        {
            setting.CurrentValue = GearsConversions.FromBool(modConfigValue);
            GearsModAPI.SaveGlobalSettings();
        }
    }

    private static void ConfigureRangeSetting(IGlobalModSettingsCategory generalCategory)
    {
        if (!TryGetGlobalValueSetting(generalCategory, "Range", out IGlobalValueSetting setting))
        {
            return;
        }

        setting.OnSettingChanged += SetRange;
        SyncRangeSetting(setting);
    }

    private static void SetRange(IGlobalModSetting setting, string newValue)
    {
        var value = GearsConversions.ToFloat(newValue, ModConfig.MIN_RANGE);
        var oldValue = ModConfig.ClientConfig.range;

        if (oldValue != value)
        {
            ModConfig.ClientConfig.range = value;
            ModConfig.SaveConfig();
        }
    }
    private static void SyncRangeSetting(IGlobalValueSetting setting)
    {
        var modConfigValue = ModConfig.ClientConfig.range;

        if (!GearsConversions.IsEqualValue(setting.CurrentValue, modConfigValue))
        {
            setting.CurrentValue = GearsConversions.FromFloat(modConfigValue, ModConfig.MIN_RANGE, ModConfig.MAX_RANGE);
            GearsModAPI.SaveGlobalSettings();
        }
    }

    private static void ConfigureIncludeDronesSetting(IGlobalModSettingsCategory generalCategory)
    {
        if (!TryGetGlobalValueSetting(generalCategory, "IncludeDrones", out IGlobalValueSetting setting))
        {
            return;
        }

        setting.OnSettingChanged += SetIncludeDrones;
        SyncIncludeDronesSetting(setting);
    }

    private static void SetIncludeDrones(IGlobalModSetting setting, string newValue)
    {
        var value = GearsConversions.ToBool(newValue, true);
        var oldValue = ModConfig.ClientConfig.includeDrones;

        if (oldValue != value)
        {
            ModConfig.ClientConfig.includeDrones = value;
            ModConfig.SaveConfig();
        }
    }

    private static void SyncIncludeDronesSetting(IGlobalValueSetting setting)
    {
        var modConfigValue = ModConfig.ClientConfig.includeDrones;

        if (!GearsConversions.IsEqualValue(setting.CurrentValue, modConfigValue))
        {
            setting.CurrentValue = GearsConversions.FromBool(modConfigValue);
            GearsModAPI.SaveGlobalSettings();
        }
    }

    private static void ConfigureIncludeVehiclesSetting(IGlobalModSettingsCategory generalCategory)
    {
        if (!TryGetGlobalValueSetting(generalCategory, "IncludeVehicles", out IGlobalValueSetting setting))
        {
            return;
        }

        setting.OnSettingChanged += SetIncludeVehicles;
        SyncIncludeVehiclesSetting(setting);
    }

    private static void SetIncludeVehicles(IGlobalModSetting setting, string newValue)
    {
        var value = GearsConversions.ToBool(newValue, true);
        var oldValue = ModConfig.ClientConfig.includeVehicles;

        if (oldValue != value)
        {
            ModConfig.ClientConfig.includeVehicles = value;
            ModConfig.SaveConfig();
        }
    }

    private static void SyncIncludeVehiclesSetting(IGlobalValueSetting setting)
    {
        var modConfigValue = ModConfig.ClientConfig.includeVehicles;

        if (!GearsConversions.IsEqualValue(setting.CurrentValue, modConfigValue))
        {
            setting.CurrentValue = GearsConversions.FromBool(modConfigValue);
            GearsModAPI.SaveGlobalSettings();
        }
    }

    private static void ConfigureIsDebugSetting(IGlobalModSettingsCategory generalCategory)
    {
        if (!TryGetGlobalValueSetting(generalCategory, "IsDebug", out IGlobalValueSetting setting))
        {
            return;
        }

        setting.OnSettingChanged += SetIsDebug;
        SyncIsDebugSetting(setting);
    }

    private static void SetIsDebug(IGlobalModSetting setting, string newValue)
    {
        var value = GearsConversions.ToBool(newValue, false);
        var oldValue = ModConfig.ClientConfig.isDebug;

        if (oldValue != value)
        {
            ModConfig.ClientConfig.isDebug = value;
            ModConfig.SaveConfig();
        }
    }
    private static void SyncIsDebugSetting(IGlobalValueSetting setting)
    {
        var modConfigValue = ModConfig.ClientConfig.isDebug;

        if (!GearsConversions.IsEqualValue(setting.CurrentValue, modConfigValue))
        {
            setting.CurrentValue = GearsConversions.FromBool(modConfigValue);
            GearsModAPI.SaveGlobalSettings();
        }
    }
}
