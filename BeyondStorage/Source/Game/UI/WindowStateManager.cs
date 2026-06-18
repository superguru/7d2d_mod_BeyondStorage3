using BeyondStorage.Infrastructure;

namespace BeyondStorage.Game.UI;

/// <summary>
/// Manages the state of various UI windows for tracking open containers and storage interfaces
/// </summary>
public static class WindowStateManager
{
    // Collectors
    private static readonly object s_collectorLockObject = new();
    private static XUiC_DewCollectorWindowGroup s_collectorWindowInstance = null;

    // Lootable windows, sach as Player Inventory, Player Crafted Storage, Storage Crates
    private static readonly object s_lootLockObject = new();
    private static XUiC_BackpackWindow s_backpackWindowInstance = null;
    private static XUiC_LootWindow s_lootWindowInstance = null;
    private static bool s_isPlayerStorageWindowOpen = false;

    // Vehicles + Drones
    private static readonly object s_bagStorageWindowLockObject = new();
    private static XUiC_BagStorageWindowGroup s_bagStorageWindowInstance = null;

    // Workstations
    private static readonly object s_workstationLockObject = new();
    private static XUiC_WorkstationWindowGroup s_workstationWindowInstance = null;

    // Entities that can be active when certain windows are opened. Always update by calling SetOpenWindowEntities()
    private static readonly object s_windowEntityLockObject = new();
    private static EntityVehicle s_vehicleForWindow;
    private static EntityDrone s_droneForWindow;

    #region Bag Storage Window

    public static XUiC_BagStorageWindowGroup GetBagStorageWindow()
    {
        lock (s_bagStorageWindowLockObject)
        {
            return s_bagStorageWindowInstance;
        }
    }

    public static bool IsBagStorageWindowOpen()
    {
        lock (s_bagStorageWindowLockObject)
        {
            return s_bagStorageWindowInstance != null;
        }
    }

    internal static void OnBagStorageWindowOpened(XUiC_BagStorageWindowGroup window)
    {
        lock (s_bagStorageWindowLockObject)
        {
            if (window == null)
            {
                ModLogger.Warning($"[WindowStateManager] Cannot track null Bag Storage window");
            }
            else if (s_bagStorageWindowInstance != null)
            {
                ModLogger.Warning($"[WindowStateManager] Cannot track null a second Bag Storage window");
            }

            s_bagStorageWindowInstance = window;

            SetOpenWindowEntities(window?.Entity);
        }
    }

    internal static void OnBagStorageWindowClosed(XUiC_BagStorageWindowGroup window)
    {
        lock (s_bagStorageWindowLockObject)
        {
            if (window == null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close a null bag storage window");
            }
            else if (s_bagStorageWindowInstance == null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close bag storage window but there isn't one open");
            }
            else if (s_bagStorageWindowInstance != window)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close bag storage window that doesn't match tracked instance");
            }

            s_bagStorageWindowInstance = null;
            SetOpenWindowEntities(null);
        }
    }

    internal static EntityDrone GetOpenWindowDrone()
    {
        lock (s_bagStorageWindowLockObject)
        {
            lock (s_bagStorageWindowLockObject)
            {
                return s_bagStorageWindowInstance == null ? null : s_droneForWindow;
            }
        }
    }

    internal static EntityVehicle GetOpenWindowVehicle()
    {
        lock (s_bagStorageWindowLockObject)
        {
            lock (s_windowEntityLockObject)
            {
                return s_bagStorageWindowInstance == null ? null : s_vehicleForWindow;
            }
        }
    }

    private static void SetOpenWindowDrone(Entity entity)
    {
        lock (s_windowEntityLockObject)
        {
            if (entity is EntityDrone drone)
            {
                s_droneForWindow = drone;
            }
            else
            {
                s_droneForWindow = null;
            }
        }
    }

    private static void SetOpenWindowVehicle(Entity entity)
    {
        lock (s_windowEntityLockObject)
        {
            if (entity is EntityVehicle vehicle)
            {
                s_vehicleForWindow = vehicle;
            }
            else
            {
                s_vehicleForWindow = null;
            }
        }
    }

    private static void SetOpenWindowEntities(Entity entity)
    {
        // Keep SetOpenWindowEntitiesModified in sync with this
        SetOpenWindowDrone(entity);
        SetOpenWindowVehicle(entity);
    }

    internal static void SetOpenWindowEntitiesModified()
    {
        // Keep SetOpenWindowEntities in sync with this
        lock (s_bagStorageWindowLockObject)
        {
            if (s_bagStorageWindowInstance != null)
            {
                if (GetOpenWindowDrone != null || GetOpenWindowVehicle() != null)
                {
                    s_bagStorageWindowInstance.IsDirty = true;
                    s_bagStorageWindowInstance.SetAllChildrenDirty();

                    var bag = s_bagStorageWindowInstance.Bag;
                    bag?.onBackpackChanged();
                }
            }
        }
    }

    #endregion

    #region Storage Container (Loot) Window

    /// <summary>
    /// Gets whether a storage container window is currently open
    /// </summary>
    /// <returns>True if a storage container window is open, false otherwise</returns>
    /// <remarks>
    /// This method only returns true for storage containers (chests, safes, etc.) and drones.
    /// Random loot containers in the world (abandoned cars, dumpsters, etc.) are not considered storage.
    /// </remarks>
    public static bool IsPlayerStorageOpen()
    {
        lock (s_lootLockObject)
        {
            // If it isn't storage, then it's some random loot container out in the world.
            // Maybe an abandoned car. Maybe a dumpster. Who knows?
            return s_isPlayerStorageWindowOpen;
        }
    }

    /// <summary>
    /// Gets the currently active storage container window instance
    /// </summary>
    /// <returns>The active storage container window instance, or null if none is open</returns>
    public static XUiC_LootWindow GetActiveStorageContainerWindow()
    {
        lock (s_lootLockObject)
        {
            return s_isPlayerStorageWindowOpen ? s_lootWindowInstance : null;
        }
    }

    /// <summary>
    /// Gets the lootable tile entity associated with the currently open storage container window
    /// </summary>
    /// <returns>The active lootable tile entity, or null if no storage container window is open</returns>
    internal static ITileEntityLootable GetOpenWindowLootable()
    {
        var lootWindow = GetActiveStorageContainerWindow();
        return lootWindow?.te;
    }

    public static bool IsLootContainerWindowOpen()
    {
        lock (s_lootLockObject)
        {
            return s_lootWindowInstance != null;
        }
    }

    internal static void OnStorageWindowOpened(XUiC_LootWindow window, bool isStorage)
    {
        lock (s_lootLockObject)
        {
            if (s_isPlayerStorageWindowOpen || (s_lootWindowInstance != null))
            {
                // Log warning and reset state to prevent confusion - this can happen with multiple containers
                ModLogger.Warning($"[WindowStateManager] Storage container window opened while another was already tracked. Resetting state. Previous: {s_lootWindowInstance?.GetType().Name}, New: {window?.GetType().Name}");
                s_isPlayerStorageWindowOpen = false;
                s_lootWindowInstance = null;
                s_droneForWindow = null;
            }

            s_lootWindowInstance = window;
            s_isPlayerStorageWindowOpen = isStorage;
        }
    }

    /// <summary>
    /// Called when a storage container window closes
    /// </summary>
    /// <param name="window">The storage container window that closed</param>
    internal static void OnStorageWindowClosed(XUiC_LootWindow window)
    {
        lock (s_lootLockObject)
        {
            // Only clear state if the closing window is the currently tracked one
            if (window == s_lootWindowInstance)
            {
                s_lootWindowInstance = null;
                s_isPlayerStorageWindowOpen = false;
                s_droneForWindow = null;
            }
            else if (s_lootWindowInstance != null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close storage container window that doesn't match tracked instance.");
            }
        }
    }

    #endregion

    #region Backpack Window

    /// <summary>
    /// Gets the currently active backpack window instance
    /// </summary>
    /// <returns>The active backpack window instance, or null if none is open</returns>
    public static XUiC_BackpackWindow GetActiveBackpackWindow()
    {
        lock (s_lootLockObject)
        {
            return s_backpackWindowInstance;
        }
    }

    /// <summary>
    /// Gets whether the player backpack window is currently open
    /// </summary>
    /// <returns>True if the backpack window is open, false otherwise</returns>
    public static bool IsBackpackWindowOpen()
    {
        lock (s_lootLockObject)
        {
            return s_backpackWindowInstance != null;
        }
    }

    public static string IsOnlyPlayerBackpackOpen()
    {
        // Player backpack window has not actually opened yet when this is called from GetBindingValue. It is about to open.
        // A workstation/collector does not have a "loot window" that opens for it, there is just the fuel and output, so by this logic,
        // a workstation or collector falls under the "only backpack open" category, which is what we want.

        bool result =
            !IsDroneWindowOpen() &&
            !IsVehicleWindowOpen() &&
            !IsPlayerStorageOpen() &&
            !IsLootContainerWindowOpen();

#if DEBUG
        //ModLogger.DebugLog($"IsPlayerBackpackOpenOnly: {result} (Drone: {IsDroneWindowOpen()}, Vehicle: {IsVehicleWindowOpen()}, Workstation: {IsWorkstationWindowOpen()}, Collector: {IsCollectorWindowOpen()}, PlayerStorage: {IsPlayerStorageOpen()}, LootContainer: {IsLootContainerWindowOpen()})");
#endif

        return result.ToString();
    }

    internal static bool IsVehicleWindowOpen()
    {
        return GetOpenWindowVehicle() != null;
    }


    /// <summary>
    /// Called when a backpack window opens
    /// </summary>
    /// <param name="backpackWindow">The backpack window that opened</param>
    internal static void OnBackpackWindowOpened(XUiC_BackpackWindow backpackWindow)
    {
        lock (s_lootLockObject)
        {
            if (s_backpackWindowInstance != null)
            {
                // Log warning and reset state to prevent confusion - this can happen with multiple containers
                ModLogger.Warning($"[WindowStateManager] Backpack window opened while another was already tracked. Resetting state. Previous: {s_backpackWindowInstance?.GetType().Name}, New: {backpackWindow?.GetType().Name}");
            }

            s_backpackWindowInstance = backpackWindow;
        }
    }

    /// <summary>
    /// Called when a backpack window closes
    /// </summary>
    /// <param name="backpackWindow">The backpack window that closed</param>
    internal static void OnBackpackWindowClosed(XUiC_BackpackWindow backpackWindow)
    {
        lock (s_lootLockObject)
        {
            // Only clear state if the closing window is the currently tracked one
            if (backpackWindow != s_backpackWindowInstance && s_backpackWindowInstance != null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close backpack window that doesn't match tracked instance.");
            }

            s_backpackWindowInstance = null;
        }
    }

    #endregion

    #region Drone Detection

    /// <summary>
    /// Gets whether a drone storage window is currently open
    /// </summary>
    /// <returns>True if a storage container window is open and it belongs to a drone, false otherwise</returns>
    public static bool IsDroneWindowOpen()
    {
        lock (s_windowEntityLockObject)
        {
            return s_droneForWindow != null;
        }
    }

    #endregion

    #region Workstation Window

    /// <summary>
    /// Gets whether a workstation window is currently open
    /// </summary>
    /// <returns>True if a workstation window is open, false otherwise</returns>
    public static bool IsWorkstationWindowOpen()
    {
        lock (s_workstationLockObject)
        {
            return s_workstationWindowInstance != null;
        }
    }

    /// <summary>
    /// Gets the currently active workstation window instance
    /// </summary>
    /// <returns>The active workstation window instance, or null if none is open</returns>
    public static XUiC_WorkstationWindowGroup GetActiveWorkstationWindow()
    {
        lock (s_workstationLockObject)
        {
            return s_workstationWindowInstance;
        }
    }

    /// <summary>
    /// Checks if the specified workstation window is the currently active one
    /// </summary>
    /// <param name="window">The window to check</param>
    /// <returns>True if the window is the currently active workstation window</returns>
    public static bool IsCurrentlyActiveWorkstationWindow(XUiC_WorkstationWindowGroup window)
    {
        lock (s_workstationLockObject)
        {
            return s_workstationWindowInstance != null && s_workstationWindowInstance == window;
        }
    }

    /// <summary>
    /// Called when a workstation window opens
    /// </summary>
    /// <param name="window">The workstation window that opened</param>
    internal static void OnWorkstationWindowOpened(XUiC_WorkstationWindowGroup window)
    {
        lock (s_workstationLockObject)
        {
            if (s_workstationWindowInstance != null)
            {
                // Log warning and reset state to prevent confusion
                ModLogger.Warning($"[WindowStateManager] Workstation window opened while another was already tracked. Resetting state. Previous: {s_workstationWindowInstance?.GetType().Name}, New: {window?.GetType().Name}");
                s_workstationWindowInstance = null;
            }

            s_workstationWindowInstance = window;
        }
    }

    /// <summary>
    /// Called when a workstation window closes
    /// </summary>
    /// <param name="window">The workstation window that closed</param>
    internal static void OnWorkstationWindowClosed(XUiC_WorkstationWindowGroup window)
    {
        lock (s_workstationLockObject)
        {
            // Only clear state if the closing window is the currently tracked one
            if (s_workstationWindowInstance == window)
            {
                s_workstationWindowInstance = null;
            }
            else if (s_workstationWindowInstance != null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close workstation window that doesn't match tracked instance.");
            }
        }
    }

    internal static TileEntityWorkstation GetOpenWorkstationTileEntity()
    {
        var workstationWindow = GetActiveWorkstationWindow();
        return workstationWindow?.WorkstationData?.TileEntity;
    }

    #endregion

    #region Collector Window

    /// <summary>
    /// Gets the currently active dew collector window instance
    /// </summary>
    /// <returns>The active collector window instance, or null if none is open</returns>
    public static XUiC_DewCollectorWindowGroup GetActiveCollectorWindow()
    {
        lock (s_collectorLockObject)
        {
            return s_collectorWindowInstance;
        }
    }

    /// <summary>
    /// Gets whether a dew collector window is currently open
    /// </summary>
    /// <returns>True if a collector window is open, false otherwise</returns>
    public static bool IsCollectorWindowOpen()
    {
        lock (s_collectorLockObject)
        {
            return s_collectorWindowInstance != null;
        }
    }

    /// <summary>
    /// Called when a dew collector window opens
    /// </summary>
    /// <param name="window">The dew collector window that opened</param>
    internal static void OnCollectorWindowOpened(XUiC_DewCollectorWindowGroup window)
    {
        lock (s_collectorLockObject)
        {
            if (s_collectorWindowInstance != null)
            {
                // Log warning and reset state to prevent confusion
                ModLogger.Warning($"[WindowStateManager] Collector window opened while another was already tracked. Resetting state. Previous: {s_collectorWindowInstance?.GetType().Name}, New: {window?.GetType().Name}");
                s_collectorWindowInstance = null;
            }

            s_collectorWindowInstance = window;
        }
    }

    /// <summary>
    /// Called when a dew collector window closes
    /// </summary>
    /// <param name="window">The dew collector window that closed</param>
    internal static void OnCollectorWindowClosed(XUiC_DewCollectorWindowGroup window)
    {
        lock (s_collectorLockObject)
        {
            // Only clear state if the closing window is the currently tracked one
            if (s_collectorWindowInstance == window)
            {
                s_collectorWindowInstance = null;
            }
            else if (s_collectorWindowInstance != null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close collector window that doesn't match tracked instance.");
            }
        }
    }

    public static TileEntityCollector GetOpenCollectorTileEntity()
    {
        var collectorWindow = GetActiveCollectorWindow();
        return collectorWindow?.te;
    }

    #endregion
}