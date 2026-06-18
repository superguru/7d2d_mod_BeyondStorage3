using BeyondStorage.Infrastructure;

namespace BeyondStorage.Game.UI;

/// <summary>
/// Tracks which UI windows are currently open and which game entities are associated with them.
/// All state is guarded by per-category locks; use the public API rather than reading fields directly.
/// </summary>
public static class WindowStateManager
{
    // Collectors
    private static readonly object s_collectorLockObject = new();
    private static XUiC_DewCollectorWindowGroup s_collectorWindowInstance = null;

    // Lootable windows: Player Inventory, Player Crafted Storage, Storage Crates
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

    // Entities associated with the currently open bag storage window.
    // Always update via SetOpenWindowEntities().
    private static readonly object s_windowEntityLockObject = new();
    private static EntityVehicle s_vehicleForWindow;
    private static EntityDrone s_droneForWindow;

    #region Bag Storage Window

    /// <summary>
    /// Gets the currently active bag storage window instance
    /// </summary>
    /// <returns>The active bag storage window instance, or null if none is open</returns>
    public static XUiC_BagStorageWindowGroup GetBagStorageWindow()
    {
        lock (s_bagStorageWindowLockObject)
        {
            return s_bagStorageWindowInstance;
        }
    }

    /// <summary>
    /// Gets whether a bag storage window (vehicle or drone) is currently open
    /// </summary>
    /// <returns>True if a bag storage window is open, false otherwise</returns>
    public static bool IsBagStorageWindowOpen()
    {
        lock (s_bagStorageWindowLockObject)
        {
            return s_bagStorageWindowInstance != null;
        }
    }

    /// <summary>
    /// Gets whether a vehicle storage window is currently open
    /// </summary>
    /// <returns>True if the open bag storage window is associated with a vehicle</returns>
    internal static bool IsVehicleWindowOpen()
    {
        return GetOpenWindowVehicle() != null;
    }

    /// <summary>
    /// Gets the vehicle associated with the currently open bag storage window
    /// </summary>
    /// <returns>The vehicle entity, or null if no vehicle bag storage window is open</returns>
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

    /// <summary>
    /// Gets the drone associated with the currently open bag storage window
    /// </summary>
    /// <returns>The drone entity, or null if no drone bag storage window is open</returns>
    internal static EntityDrone GetOpenWindowDrone()
    {
        lock (s_bagStorageWindowLockObject)
        {
            lock (s_windowEntityLockObject)
            {
                return s_bagStorageWindowInstance == null ? null : s_droneForWindow;
            }
        }
    }

    /// <summary>
    /// Called when a bag storage window opens
    /// </summary>
    /// <param name="window">The bag storage window that opened</param>
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
                ModLogger.Warning($"[WindowStateManager] Cannot track a second Bag Storage window while one is already open");
            }

            s_bagStorageWindowInstance = window;
            SetOpenWindowEntities(window?.Entity);
        }
    }

    /// <summary>
    /// Called when a bag storage window closes
    /// </summary>
    /// <param name="window">The bag storage window that closed</param>
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

    /// <summary>
    /// Marks the open bag storage window and its associated entity as dirty, triggering a UI refresh
    /// Note: Keep SetOpenWindowEntities in sync with this!
    /// </summary>
    internal static void SetOpenWindowEntitiesModified()
    {
        lock (s_bagStorageWindowLockObject)
        {
            if (s_bagStorageWindowInstance != null)
            {
                if (GetOpenWindowDrone() != null || GetOpenWindowVehicle() != null)
                {
                    s_bagStorageWindowInstance.IsDirty = true;
                    s_bagStorageWindowInstance.SetAllChildrenDirty();

                    var bag = s_bagStorageWindowInstance.Bag;
                    bag?.onBackpackChanged();
                }
            }
        }
    }

    private static void SetOpenWindowEntities(Entity entity)
    {
        SetOpenWindowDrone(entity);
        SetOpenWindowVehicle(entity);
    }

    private static void SetOpenWindowDrone(Entity entity)
    {
        lock (s_windowEntityLockObject)
        {
            s_droneForWindow = entity as EntityDrone;
        }
    }

    private static void SetOpenWindowVehicle(Entity entity)
    {
        lock (s_windowEntityLockObject)
        {
            s_vehicleForWindow = entity as EntityVehicle;
        }
    }

    #endregion

    #region Storage Container (Loot) Window

    /// <summary>
    /// Gets whether a storage container window is currently open
    /// </summary>
    /// <returns>True if a storage container window is open, false otherwise</returns>
    /// <remarks>
    /// Only returns true for storage containers (chests, safes, etc.) and drones.
    /// Random loot containers in the world (abandoned cars, dumpsters, etc.) are not considered storage.
    /// </remarks>
    public static bool IsPlayerStorageOpen()
    {
        lock (s_lootLockObject)
        {
            return s_isPlayerStorageWindowOpen;
        }
    }

    /// <summary>
    /// Gets whether any loot container window is currently open, including non-storage world containers
    /// </summary>
    /// <returns>True if any loot window is open, false otherwise</returns>
    public static bool IsLootContainerWindowOpen()
    {
        lock (s_lootLockObject)
        {
            return s_lootWindowInstance != null;
        }
    }

    /// <summary>
    /// Gets the currently active storage container window instance
    /// </summary>
    /// <returns>The active storage container window, or null if none is open or open window is not player storage</returns>
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

    /// <summary>
    /// Called when a loot window opens
    /// </summary>
    /// <param name="window">The loot window that opened</param>
    /// <param name="isStorage">True if the container is player-owned storage rather than world loot</param>
    internal static void OnStorageWindowOpened(XUiC_LootWindow window, bool isStorage)
    {
        lock (s_lootLockObject)
        {
            if (s_isPlayerStorageWindowOpen || s_lootWindowInstance != null)
            {
                ModLogger.Warning($"[WindowStateManager] Storage container window opened while another was already tracked. Resetting state. Previous: {s_lootWindowInstance?.GetType().Name}, New: {window?.GetType().Name}");
                s_isPlayerStorageWindowOpen = false;
                s_lootWindowInstance = null;
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
            if (window == s_lootWindowInstance)
            {
                s_lootWindowInstance = null;
                s_isPlayerStorageWindowOpen = false;
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

    /// <summary>
    /// Gets whether only the player backpack is open with no other container or vehicle window active.
    /// Returns a string for XUI data binding.
    /// </summary>
    /// <remarks>
    /// Workstations and collectors count as "only backpack open" since they have no loot window.
    /// The backpack window itself has not yet opened when XUI calls this from GetBindingValue.
    /// </remarks>
    public static string IsOnlyPlayerBackpackOpen()
    {
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
    /// Gets whether a drone bag storage window is currently open
    /// </summary>
    /// <returns>True if a drone bag storage window is open, false otherwise</returns>
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
    /// Gets the tile entity associated with the currently open workstation window
    /// </summary>
    /// <returns>The workstation tile entity, or null if no workstation window is open</returns>
    internal static TileEntityWorkstation GetOpenWorkstationTileEntity()
    {
        var workstationWindow = GetActiveWorkstationWindow();
        return workstationWindow?.WorkstationData?.TileEntity;
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

    #endregion

    #region Collector Window

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
    /// Gets the tile entity associated with the currently open collector window
    /// </summary>
    /// <returns>The collector tile entity, or null if no collector window is open</returns>
    public static TileEntityCollector GetOpenCollectorTileEntity()
    {
        var collectorWindow = GetActiveCollectorWindow();
        return collectorWindow?.te;
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

    #endregion
}
