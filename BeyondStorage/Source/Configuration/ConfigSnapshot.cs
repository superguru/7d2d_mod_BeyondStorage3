using Newtonsoft.Json;

namespace BeyondStorage.Configuration;

/// <summary>
/// Configuration snapshot that captures all relevant settings at a single point in time
/// to ensure consistency throughout method execution.
/// </summary>
public sealed class ConfigSnapshot
{
    // ========== Source selection / eligibility =========
    public float Range
    {
        get;
    }
    public bool AllowPushToAlliedVehicles
    {
        get;
    }
    public bool IncludeDrones
    {
        get;
    }
    public bool IncludeVehicles
    {
        get;
    }

    // ========== Useables =========
    public bool ShowUseables
    {
        get;
    }

    // ========== Housekeeping =========
    public bool IsDebug
    {
        get;
    }

    private ConfigSnapshot()
    {
        // ========== Source selection / eligibility =========
        Range = ModConfig.Range();
        AllowPushToAlliedVehicles = ModConfig.AllowPushToAlliedVehicles();
        IncludeDrones = ModConfig.IncludeDrones();
        IncludeVehicles = ModConfig.IncludeVehicles();

        // ========== Housekeeping =========
        IsDebug = ModConfig.IsDebug();
        ShowUseables = ModConfig.ShowUseables();
    }

    public static ConfigSnapshot Current => new();

    /// <summary>
    /// Returns a pretty-printed JSON representation of all configuration options as a flat list.
    /// </summary>
    /// <returns>Formatted JSON string containing all configuration attributes</returns>
    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}