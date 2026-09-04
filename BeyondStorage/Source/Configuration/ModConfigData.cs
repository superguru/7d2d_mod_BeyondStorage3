using BeyondStorage.Infrastructure;
using Newtonsoft.Json;

namespace BeyondStorage.Configuration;

/// <summary>
/// Configuration class for Beyond Storage mod settings
/// </summary>
public class ModConfigData
{
    /// <summary>
    /// Optional metadata description field for configuration documentation purposes
    /// </summary>
    [JsonProperty(nameof(metaDescription))]
    public string metaDescription = string.Empty;

    // ========== Versioning =========
    /// <summary>
    /// Config schema version - matches ModInfo.Version when config was created/migrated
    /// </summary>
    [JsonProperty(nameof(version))]
    public string version = ConfigVersioning.CurrentVersion;

    // ========== Source selection / eligibility =========
    /// <summary>
    /// How far to pull from (0 is infinite range, only limited by chunks loaded)
    /// </summary>
    [JsonProperty(nameof(range))]
    public float range = 0f;

    /// <summary>
    /// If set to true it will try and pull items from nearby drones
    /// </summary>
    [JsonProperty(nameof(includeDrones))]
    public bool includeDrones = true;

    /// <summary>
    /// If set to true it will try and pull items from nearby vehicle storages
    /// </summary>
    [JsonProperty(nameof(includeVehicles))]
    public bool includeVehicles = true;

    [JsonProperty(nameof(allowPushToAlliedVehicles))]
    public bool allowPushToAlliedVehicles = true;

    // ========== Housekeeping =========
    /// <summary>
    /// If set true additional logging will be printed to logs/console
    /// </summary>
    [JsonProperty(nameof(isDebug))]
    public bool isDebug = false;

    /// <summary>
    /// If set false, the Useables window (heal/eat/drink) is hidden.
    /// </summary>
    [JsonProperty(nameof(showUseables))]
    public bool showUseables = true;
}