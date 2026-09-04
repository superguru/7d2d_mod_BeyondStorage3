using System.Collections.Generic;

namespace BeyondStorage.Configuration.Gears;

internal static class GearsSettingsRegistry
{
    private const string GeneralTab = "General";

    private const string GeneralCategory = "General";

    public static readonly (string Tab, string Category, IReadOnlyList<IGearsSetting> Settings)[] Entries =
    [
        (GeneralTab, GeneralCategory,
        [
            GearsSettingFactory.Float(
                nameof(ModConfig.Range),
                c => c.range,
                (c, v) => c.range = v,
                ModConfig.MIN_RANGE, ModConfig.MAX_RANGE, ModConfig.DEFAULT_RANGE),
            GearsSettingFactory.Bool(
                nameof(ModConfig.IncludeDrones),
                c => c.includeDrones,
                (c, v) => c.includeDrones = v),
            GearsSettingFactory.Bool(
                nameof(ModConfig.IncludeVehicles),
                c => c.includeVehicles,
                (c, v) => c.includeVehicles = v),
            GearsSettingFactory.Bool(
                nameof(ModConfig.AllowPushToAlliedVehicles),
                c => c.allowPushToAlliedVehicles,
                (c, v) => c.allowPushToAlliedVehicles = v),
            GearsSettingFactory.Bool(
                nameof(ModConfig.ShowUseables),
                c => c.showUseables,
                (c, v) => c.showUseables = v),
            GearsSettingFactory.Bool(
                nameof(ModConfig.IsDebug),
                c => c.isDebug,
                (c, v) => c.isDebug = v),
        ]),
    ];
}
