using BeyondStorage.Configuration;
using BeyondStorage.Infrastructure;
using GearsAPI.Settings.Global;

namespace BeyondStorage.Source.Configuration.Gears;

internal static class GearsGeneralSettings
{
    internal static void Configure(IGlobalModSettingsCategory generalCategory)
    {
        ConfigureRangeSetting(generalCategory);
        ConfigureAllowPushToAlliedVehiclesSetting(generalCategory);
        ConfigureConsumeFromDronesSetting(generalCategory);
        ConfigureConsumeFromVehiclesSetting(generalCategory);
        ConfigureIsDebugSetting(generalCategory);
    }

    internal static void ConfigureAllowPushToAlliedVehiclesSetting(IGlobalModSettingsCategory generalCategory)
    {
        var setting = (generalCategory.GetSetting("AllowPushToAlliedVehicles") as IGlobalValueSetting);
        if (setting == null)
        {
            ModLogger.DebugLog($"Global settings loaded, but setting is null");
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

    internal static void ConfigureRangeSetting(IGlobalModSettingsCategory generalCategory)
    {
        var setting = (generalCategory.GetSetting("Range") as IGlobalValueSetting);
        if (setting == null)
        {
            ModLogger.DebugLog($"Global settings loaded, but setting is null");
            return;
        }

        setting.OnSettingChanged += SetRange;
        SyncRangeSetting(setting);
    }

    private static void SetRange(IGlobalModSetting setting, string newValue)
    {
        var value = GearsConversions.ToFloat(newValue, ModConfig.RANGE_UNLIMITED);
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
            setting.CurrentValue = GearsConversions.FromFloat(modConfigValue);
            GearsModAPI.SaveGlobalSettings();
        }
    }

    internal static void ConfigureConsumeFromDronesSetting(IGlobalModSettingsCategory generalCategory)
    {
        var setting = (generalCategory.GetSetting("ConsumeFromDrones") as IGlobalValueSetting);
        if (setting == null)
        {
            ModLogger.DebugLog($"Global settings loaded, but setting is null");
            return;
        }

        setting.OnSettingChanged += SetConsumeFromDrones;
        SyncConsumeFromDronesSetting(setting);
    }

    private static void SetConsumeFromDrones(IGlobalModSetting setting, string newValue)
    {
        var value = GearsConversions.ToBool(newValue, true);
        var oldValue = ModConfig.ClientConfig.consumeFromDrones;

        if (oldValue != value)
        {
            ModConfig.ClientConfig.consumeFromDrones = value;
            ModConfig.SaveConfig();
        }
    }

    private static void SyncConsumeFromDronesSetting(IGlobalValueSetting setting)
    {
        var modConfigValue = ModConfig.ClientConfig.consumeFromDrones;

        if (!GearsConversions.IsEqualValue(setting.CurrentValue, modConfigValue))
        {
            setting.CurrentValue = GearsConversions.FromBool(modConfigValue);
            GearsModAPI.SaveGlobalSettings();
        }
    }

    internal static void ConfigureConsumeFromVehiclesSetting(IGlobalModSettingsCategory generalCategory)
    {
        var setting = (generalCategory.GetSetting("ConsumeFromVehicles") as IGlobalValueSetting);
        if (setting == null)
        {
            ModLogger.DebugLog($"Global settings loaded, but setting is null");
            return;
        }

        setting.OnSettingChanged += SetConsumeFromVehicles;
        SyncConsumeFromVehiclesSetting(setting);
    }

    private static void SetConsumeFromVehicles(IGlobalModSetting setting, string newValue)
    {
        var value = GearsConversions.ToBool(newValue, true);
        var oldValue = ModConfig.ClientConfig.consumeFromVehicles;

        if (oldValue != value)
        {
            ModConfig.ClientConfig.consumeFromVehicles = value;
            ModConfig.SaveConfig();
        }
    }

    private static void SyncConsumeFromVehiclesSetting(IGlobalValueSetting setting)
    {
        var modConfigValue = ModConfig.ClientConfig.consumeFromVehicles;

        if (!GearsConversions.IsEqualValue(setting.CurrentValue, modConfigValue))
        {
            setting.CurrentValue = GearsConversions.FromBool(modConfigValue);
            GearsModAPI.SaveGlobalSettings();
        }
    }

    internal static void ConfigureIsDebugSetting(IGlobalModSettingsCategory generalCategory)
    {
        var setting = (generalCategory.GetSetting("IsDebug") as IGlobalValueSetting);
        if (setting == null)
        {
            ModLogger.DebugLog($"Global settings loaded, but setting is null");
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
