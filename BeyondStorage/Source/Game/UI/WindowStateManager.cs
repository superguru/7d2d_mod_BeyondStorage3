using BeyondStorage.Harmony.Components;
using BeyondStorage.Infrastructure;
using BeyondStorage.Storage;

namespace BeyondStorage.Game.UI;

/// <summary>
/// Tracks which UI windows are currently open and which game entities are associated with them.
/// All state is guarded by per-category locks; use the public API rather than reading fields directly.
/// </summary>
public static class WindowStateManager
{
    // Collectors
    private static readonly object s_collectorLock = new();
    private static XUiC_DewCollectorWindowGroup s_collectorWindow = null;

    // Dropped loot containers
    private static readonly object s_bagContainerLock = new();
    private static XUiC_BagContainer s_bagContainerInstance = null;

    // Lootable windows: Player Inventory, Player Crafted Storage, Storage Crates
    private static readonly object s_lootLock = new();
    private static XUiC_BackpackWindow s_backpackWindow = null;
    private static XUiC_LootWindow s_lootWindow = null;
    private static bool s_isPlayerStorageWindowOpen = false;

    // Vehicles + Drones
    private static readonly object s_bagStorageWindowLock = new();
    private static XUiC_BagStorageWindowGroup s_bagStorageWindow = null;

    // Workstations
    private static readonly object s_workstationLock = new();
    private static XUiC_WorkstationWindowGroup s_workstationWindow = null;

    // Useables
    private static readonly object s_useablesWindowLock = new();
    private static XUiC_BeyondStorage_UseablesWindow s_useablesWindow = null;

    // Character Frame Window (character screen: equipment/appearance with character preview)
    private static readonly object s_characterFrameWindowLock = new();
    private static XUiC_CharacterFrameWindow s_characterFrameWindow = null;

    // Trader Window (trader UI with buy/sell)
    private static readonly object s_traderWindowLock = new();
    private static XUiC_TraderWindowGroup s_traderWindow = null;

    // Quest Turn-In Window (quest reward turn-in UI)
    private static readonly object s_questTurnInWindowLock = new();
    private static XUiC_QuestTurnInWindowGroup s_questTurnInWindow = null;

    // Entities associated with the currently open bag storage window.
    // Always update via SetOpenWindowEntities().
    private static readonly object s_windowEntityLock = new();
    private static EntityDrone s_droneForWindow;
    private static EntityVehicle s_vehicleForWindow;
    private static EntityLootContainer s_droppedLootForWindow;

    #region Bag Storage Window

    /// <summary>
    /// Gets the currently active bag storage window instance
    /// </summary>
    /// <returns>The active bag storage window instance, or null if none is open</returns>
    public static XUiC_BagStorageWindowGroup GetBagStorageWindow()
    {
        lock (s_bagStorageWindowLock)
        {
            return s_bagStorageWindow;
        }
    }

    /// <summary>
    /// Gets whether a bag storage window (vehicle or drone) is currently open
    /// </summary>
    /// <returns>True if a bag storage window is open, false otherwise</returns>
    public static bool IsBagStorageWindowOpen()
    {
        lock (s_bagStorageWindowLock)
        {
            return s_bagStorageWindow != null;
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
        lock (s_bagStorageWindowLock)
        {
            lock (s_windowEntityLock)
            {
                return s_bagStorageWindow == null ? null : s_vehicleForWindow;
            }
        }
    }

    /// <summary>
    /// Gets the drone associated with the currently open bag storage window
    /// </summary>
    /// <returns>The drone entity, or null if no drone bag storage window is open</returns>
    internal static EntityDrone GetOpenWindowDrone()
    {
        lock (s_bagStorageWindowLock)
        {
            lock (s_windowEntityLock)
            {
                return s_bagStorageWindow == null ? null : s_droneForWindow;
            }
        }
    }

    internal static EntityLootContainer GetOpenWindowDroppedLoot()
    {
        lock (s_bagStorageWindowLock)
        {
            lock (s_windowEntityLock)
            {
                return s_bagStorageWindow == null ? null : s_droppedLootForWindow;
            }
        }
    }

    /// <summary>
    /// Called when a bag storage window opens
    /// </summary>
    /// <param name="window">The bag storage window that opened</param>
    internal static void OnBagStorageWindowOpening(XUiC_BagStorageWindowGroup window)
    {
        lock (s_bagStorageWindowLock)
        {
            if (window == null)
            {
                ModLogger.Warning($"[WindowStateManager] Cannot track null Bag Storage window");
            }
            else if (s_bagStorageWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Cannot track a second Bag Storage window while one is already open");
            }

            s_bagStorageWindow = window;
            SetOpenWindowEntities(window?.Entity);
        }
    }

    /// <summary>
    /// Called when a bag storage window closes
    /// </summary>
    /// <param name="window">The bag storage window that closed</param>
    internal static void OnBagStorageWindowClosing(XUiC_BagStorageWindowGroup window)
    {
        lock (s_bagStorageWindowLock)
        {
            if (window == null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close a null bag storage window");
            }
            else if (s_bagStorageWindow == null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close bag storage window but there isn't one open");
            }
            else if (s_bagStorageWindow != window)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close bag storage window that doesn't match tracked instance");
            }

            s_bagStorageWindow = null;
            SetOpenWindowEntities(null);
        }
    }

    /// <summary>
    /// Marks the open bag storage window and its associated entity as dirty, triggering a UI refresh
    /// Note: Keep SetOpenWindowEntities in sync with this!
    /// </summary>
    internal static void SetOpenWindowEntitiesModified()
    {
        lock (s_bagStorageWindowLock)
        {
            if (s_bagStorageWindow != null)
            {
                if (GetOpenWindowDrone() != null || GetOpenWindowVehicle() != null || GetOpenWindowDroppedLoot() != null)
                {
                    s_bagStorageWindow.IsDirty = true;
                    s_bagStorageWindow.SetAllChildrenDirty();

                    var bag = s_bagStorageWindow.Bag;
                    bag?.onBackpackChanged();
                }
            }
        }
    }

    private static void SetOpenWindowEntities(Entity entity)
    {
        SetOpenWindowDrone(entity);
        SetOpenWindowVehicle(entity);
        SetOpenWindowDroppedLoot(entity);
    }

    private static void SetOpenWindowDrone(Entity entity)
    {
        lock (s_windowEntityLock)
        {
            s_droneForWindow = entity as EntityDrone;
        }
    }

    private static void SetOpenWindowVehicle(Entity entity)
    {
        lock (s_windowEntityLock)
        {
            s_vehicleForWindow = entity as EntityVehicle;
        }
    }

    private static void SetOpenWindowDroppedLoot(Entity entity)
    {
        lock (s_windowEntityLock)
        {
            s_droppedLootForWindow = entity as EntityLootContainer;
        }
    }

    #endregion

    #region Storage Container (Loot) Window

    /// <summary>
    /// Gets whether a storage container window is currently open
    /// </summary>
    /// <returns>True if a storage container window is open, false otherwise</returns>
    /// <remarks>
    /// Only returns true for storage containers (chests, safes, etc.).
    /// Random loot containers in the world (abandoned cars, dumpsters, etc.) are not considered storage.
    /// Drones are tracked separately via <see cref="IsDroneWindowOpen"/>.
    /// </remarks>
    public static bool IsPlayerStorageOpen()
    {
        lock (s_lootLock)
        {
            //ModLogger.DebugLog($"IsplayerStorageOpen: IsBagStorageWindowOpen={IsBagStorageWindowOpen()}, s_bagContainerInstance='{s_bagContainerInstance?.containerName}'");
            return s_isPlayerStorageWindowOpen;
        }
    }

    internal static bool IsAnyLootWindowOpen()
    {
        lock (s_lootLock)
        {
            return s_lootWindow != null;
        }
    }

    /// <summary>
    /// Gets the currently active storage container window instance
    /// </summary>
    /// <returns>The active storage container window, or null if none is open</returns>
    internal static XUiC_LootWindow GetActiveStorageContainerWindow()
    {
        lock (s_lootLock)
        {
            return s_lootWindow;
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
    internal static void OnStorageWindowOpening(XUiC_LootWindow window, bool isStorage)
    {
#if DEBUG
        //const string d_MethodName = nameof(OnStorageWindowOpening);
#endif
        lock (s_lootLock)
        {
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: Start: s_lootWindow={s_lootWindow != null}, s_isPlayerStorageWindowOpen={s_isPlayerStorageWindowOpen}");
#endif
            if (s_isPlayerStorageWindowOpen || s_lootWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Storage container window opened while another was already tracked. Resetting state. Previous: {s_lootWindow?.GetType().Name}, New: {window?.GetType().Name}");
                s_isPlayerStorageWindowOpen = false;
                s_lootWindow = null;
            }

            s_lootWindow = window;
            s_isPlayerStorageWindowOpen = isStorage;
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: End: s_lootWindow={s_lootWindow != null}, s_isPlayerStorageWindowOpen={s_isPlayerStorageWindowOpen}");
#endif
        }
    }

    public static bool IsBagContainerOpen()
    {
        lock (s_bagContainerLock)
        {
            return s_bagContainerInstance != null;
        }
    }

    internal static void OnBagContainerOpening(XUiC_BagContainer container)
    {
#if DEBUG
        //const string d_MethodName = nameof(OnBagContainerOpening);
#endif
        lock (s_bagContainerLock)
        {
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: Start: container={container}");
#endif
            if (s_bagContainerInstance != null)
            {
                ModLogger.Warning($"[WindowStateManager] Bag container opened while another was already tracked. Resetting state. Previous: {s_bagContainerInstance?.GetType().Name}, New: {container?.GetType().Name}");
                s_bagContainerInstance = null;
            }

            s_bagContainerInstance = container;
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: End: container={container}");
#endif
        }
    }

    internal static void OnBagContainerClosing(XUiC_BagContainer container)
    {
#if DEBUG
        //const string d_MethodName = nameof(OnBagContainerClosing);
#endif
        lock (s_bagContainerLock)
        {
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: Start: container={container}");
#endif
            if (container == s_bagContainerInstance)
            {
                s_bagContainerInstance = null;
            }
            else if (s_bagContainerInstance != null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close bag container that doesn't match tracked instance.");
            }
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: End: container={container}");
#endif
        }
    }

    /// <summary>
    /// Called when a storage container window closes
    /// </summary>
    /// <param name="window">The storage container window that closed</param>
    internal static void OnStorageWindowClosing(XUiC_LootWindow window)
    {
#if DEBUG
        //const string d_MethodName = nameof(OnStorageWindowClosing);
#endif
        lock (s_lootLock)
        {
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: Start: s_lootWindow={s_lootWindow != null}, s_isPlayerStorageWindowOpen={s_isPlayerStorageWindowOpen}");
#endif
            if (window == s_lootWindow)
            {
                s_lootWindow = null;
                s_isPlayerStorageWindowOpen = false;
            }
            else if (s_lootWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close storage container window that doesn't match tracked instance.");
            }
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: End: s_lootWindow={s_lootWindow != null}, s_isPlayerStorageWindowOpen={s_isPlayerStorageWindowOpen}");
#endif
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
        lock (s_lootLock)
        {
            return s_backpackWindow;
        }
    }

    /// <summary>
    /// Gets whether the player backpack window is currently open
    /// </summary>
    /// <returns>True if the backpack window is open, false otherwise</returns>
    public static bool IsBackpackWindowOpen()
    {
        lock (s_lootLock)
        {
            return s_backpackWindow != null;
        }
    }

    /// <summary>
    /// Gets whether only the player backpack storage is open with no other container or vehicle window active.
    /// Returns a string for XUI data binding.
    /// </summary>
    /// <remarks>
    /// Workstations and collectors count as "only backpack open" since they have no loot window.
    /// The backpack window itself has not yet opened when XUI calls this from GetBindingValue.
    /// </remarks>
    public static bool IsOnlyPlayerStorageOpenInternal()
    {
        bool result =
            !IsAnyLootWindowOpen() &&
            !IsVehicleWindowOpen() &&
            !IsBagStorageWindowOpen() &&
            !IsBagContainerOpen() &&
            !IsDroneWindowOpen();

#if DEBUG
        //ModLogger.DebugLog($"IsOnlyPlayerStorageOpenInternal: {result} (L={IsAnyLootWindowOpen()}, V={isVehicleOpen}, S={IsBagStorageWindowOpen()}, B={IsBagContainerOpen()}, D={IsDroneWindowOpen()})");
#endif
        return result;
    }

    public static string IsOnlyPlayerStorageOpen()
    {
        var result = IsOnlyPlayerStorageOpenInternal();
        return result.ToString();
    }

    public static bool IsOnlyPlayerBackpackOpenInternal()
    {
        bool result =
            IsOnlyPlayerStorageOpenInternal() &&
            !IsWorkstationWindowOpen() &&
            !IsCollectorWindowOpen() &&
            !IsCharacterFrameWindowOpen() &&
            !IsTraderWindowOpen() &&
            !IsQuestTurnInWindowOpen();

#if DEBUG
        //ModLogger.DebugLog($"IsPlayerBackpackOpenOnlyInternal: {result} (P={IsOnlyPlayerStorageOpenInternal()}, W={IsWorkstationWindowOpen()}, C={IsCollectorWindowOpen()}, Char={IsCharacterFrameWindowOpen()}, Q={IsQuestTurnInWindowOpen()})");
#endif
        return result;
    }

    public static string IsOnlyPlayerBackpackOpen()
    {
        var result = IsOnlyPlayerBackpackOpenInternal();
        return result.ToString();
    }

    /// <summary>
    /// Called when a backpack window opens
    /// </summary>
    /// <param name="backpackWindow">The backpack window that opened</param>
    internal static void OnBackpackWindowOpening(XUiC_BackpackWindow backpackWindow)
    {
        lock (s_lootLock)
        {
            if (s_backpackWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Backpack window opened while another was already tracked. Resetting state. Previous: {s_backpackWindow?.GetType().Name}, New: {backpackWindow?.GetType().Name}");
            }

            s_backpackWindow = backpackWindow;
        }
    }

    /// <summary>
    /// Called when a backpack window closes
    /// </summary>
    /// <param name="backpackWindow">The backpack window that closed</param>
    internal static void OnBackpackWindowClosing(XUiC_BackpackWindow backpackWindow)
    {
        lock (s_lootLock)
        {
            if (backpackWindow != s_backpackWindow && s_backpackWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close backpack window that doesn't match tracked instance.");
            }

            s_backpackWindow = null;
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
        lock (s_windowEntityLock)
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
        lock (s_workstationLock)
        {
            return s_workstationWindow != null;
        }
    }

    /// <summary>
    /// Gets the currently active workstation window instance
    /// </summary>
    /// <returns>The active workstation window instance, or null if none is open</returns>
    public static XUiC_WorkstationWindowGroup GetActiveWorkstationWindow()
    {
        lock (s_workstationLock)
        {
            return s_workstationWindow;
        }
    }

    /// <summary>
    /// Checks if the specified workstation window is the currently active one
    /// </summary>
    /// <param name="window">The window to check</param>
    /// <returns>True if the window is the currently active workstation window</returns>
    public static bool IsCurrentlyActiveWorkstationWindow(XUiC_WorkstationWindowGroup window)
    {
        lock (s_workstationLock)
        {
            return s_workstationWindow != null && s_workstationWindow == window;
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
    internal static void OnWorkstationWindowOpening(XUiC_WorkstationWindowGroup window)
    {
        lock (s_workstationLock)
        {
            if (s_workstationWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Workstation window opened while another was already tracked. Resetting state. Previous: {s_workstationWindow?.GetType().Name}, New: {window?.GetType().Name}");
                s_workstationWindow = null;
            }

            s_workstationWindow = window;
        }
    }

    /// <summary>
    /// Called when a workstation window closes
    /// </summary>
    /// <param name="window">The workstation window that closed</param>
    internal static void OnWorkstationWindowClosing(XUiC_WorkstationWindowGroup window)
    {
        lock (s_workstationLock)
        {
            if (s_workstationWindow == window)
            {
                s_workstationWindow = null;
            }
            else if (s_workstationWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close workstation window that doesn't match tracked instance.");
            }
        }
    }

    #endregion

    #region Useables

    public static XUiC_BeyondStorage_UseablesWindow GetActiveUseablesWindow()
    {
        lock (s_useablesWindowLock)
        {
            return s_useablesWindow;
        }
    }

    public static bool IsUseablesWindowOpen()
    {
        lock (s_useablesWindowLock)
        {
            return s_useablesWindow != null;
        }
    }

    internal static void OnUseablesWindowOpening(XUiC_BeyondStorage_UseablesWindow window)
    {
        lock (s_useablesWindowLock)
        {
            if (s_useablesWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Useables window opened while another was already tracked. Resetting state. Previous: {s_useablesWindow?.GetType().Name}, New: {window?.GetType().Name}");
                s_useablesWindow = null;
            }

            s_useablesWindow = window;
        }
    }

    internal static void OnUseablesWindowClosing(XUiC_BeyondStorage_UseablesWindow window)
    {
        lock (s_useablesWindowLock)
        {
            if (s_useablesWindow == window)
            {
                s_useablesWindow = null;
            }
            else if (s_useablesWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close useables window that doesn't match tracked instance.");
            }
        }
    }

    internal static void RefreshUseablesWindowBindings()
    {
        lock (s_useablesWindowLock)
        {
            if (s_useablesWindow != null)
            {
                s_useablesWindow.RefreshBindings();
            }
        }
    }


    internal static bool ShowUseablesWindowInternal()
    {
        const string d_MethodName = nameof(ShowUseablesWindowInternal);

        if (!WorldPlayerContext.IsOkQuickCheck())
        {
            //TODO: Add proper game start and end events which we can then use to query global game state
            return false;
        }

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return false;
        }

        var result = context.Config.ShowUseables;
        return result;
    }

    internal static string ShowUseablesWindow()
    {
        var result = ShowUseablesWindowInternal();
        return result.ToString();
    }

    #endregion

    #region Collector Window

    /// <summary>
    /// Gets whether a dew collector window is currently open
    /// </summary>
    /// <returns>True if a collector window is open, false otherwise</returns>
    public static bool IsCollectorWindowOpen()
    {
        lock (s_collectorLock)
        {
            return s_collectorWindow != null;
        }
    }

    /// <summary>
    /// Gets the currently active dew collector window instance
    /// </summary>
    /// <returns>The active collector window instance, or null if none is open</returns>
    public static XUiC_DewCollectorWindowGroup GetActiveCollectorWindow()
    {
        lock (s_collectorLock)
        {
            return s_collectorWindow;
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
    internal static void OnCollectorWindowOpening(XUiC_DewCollectorWindowGroup window)
    {
        lock (s_collectorLock)
        {
            if (s_collectorWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Collector window opened while another was already tracked. Resetting state. Previous: {s_collectorWindow?.GetType().Name}, New: {window?.GetType().Name}");
                s_collectorWindow = null;
            }

            s_collectorWindow = window;
#if DEBUG
            //ModLogger.DebugLog($"[WindowStateManager] Collector window opened. block={window?.te?.blockValue.Block?.GetBlockName() ?? "null"}");
#endif
        }
    }

    /// <summary>
    /// Called when a dew collector window closes
    /// </summary>
    /// <param name="window">The dew collector window that closed</param>
    internal static void OnCollectorWindowClosing(XUiC_DewCollectorWindowGroup window)
    {
        lock (s_collectorLock)
        {
#if DEBUG
            //ModLogger.DebugLog($"[WindowStateManager] Collector window closing. block={window?.te?.blockValue.Block?.GetBlockName() ?? "null"}, tracked={s_collectorWindow != null}");
#endif
            if (s_collectorWindow == window)
            {
                s_collectorWindow = null;
            }
            else if (s_collectorWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close collector window that doesn't match tracked instance.");
            }
        }
    }

    #endregion

    #region Character Frame Window

    /// <summary>
    /// Gets whether the character frame window (character screen) is currently open
    /// </summary>
    /// <returns>True if the character frame window is open, false otherwise</returns>
    public static bool IsCharacterFrameWindowOpen()
    {
        lock (s_characterFrameWindowLock)
        {
            return s_characterFrameWindow != null;
        }
    }

    /// <summary>
    /// Gets the currently active character frame window instance
    /// </summary>
    /// <returns>The active character frame window, or null if none is open</returns>
    public static XUiC_CharacterFrameWindow GetActiveCharacterFrameWindow()
    {
        lock (s_characterFrameWindowLock)
        {
            return s_characterFrameWindow;
        }
    }

    /// <summary>
    /// Called when a character frame window opens
    /// </summary>
    /// <param name="window">The character frame window that opened</param>
    internal static void OnCharacterFrameWindowOpening(XUiC_CharacterFrameWindow window)
    {
        lock (s_characterFrameWindowLock)
        {
            if (s_characterFrameWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Character frame window opened while another was already tracked. Resetting state. Previous: {s_characterFrameWindow?.GetType().Name}, New: {window?.GetType().Name}");
                s_characterFrameWindow = null;
            }

            s_characterFrameWindow = window;
        }
    }

    /// <summary>
    /// Called when a character frame window closes
    /// </summary>
    /// <param name="window">The character frame window that closed</param>
    internal static void OnCharacterFrameWindowClosing(XUiC_CharacterFrameWindow window)
    {
        lock (s_characterFrameWindowLock)
        {
            if (s_characterFrameWindow == window)
            {
                s_characterFrameWindow = null;
            }
            else if (s_characterFrameWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close character frame window that doesn't match tracked instance.");
            }
        }
    }

    #endregion

    #region Trader Window

    /// <summary>
    /// Gets whether the trader window is currently open
    /// </summary>
    /// <returns>True if the trader window is open, false otherwise</returns>
    public static bool IsTraderWindowOpen()
    {
        lock (s_traderWindowLock)
        {
            return s_traderWindow != null;
        }
    }

    /// <summary>
    /// Gets the currently active trader window instance
    /// </summary>
    /// <returns>The active trader window, or null if none is open</returns>
    public static XUiC_TraderWindowGroup GetActiveTraderWindow()
    {
        lock (s_traderWindowLock)
        {
            return s_traderWindow;
        }
    }

    /// <summary>
    /// Called when a trader window opens
    /// </summary>
    /// <param name="window">The trader window that opened</param>
    internal static void OnTraderWindowOpening(XUiC_TraderWindowGroup window)
    {
        lock (s_traderWindowLock)
        {
            if (s_traderWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Trader window opened while another was already tracked. Resetting state. Previous: {s_traderWindow?.GetType().Name}, New: {window?.GetType().Name}");
                s_traderWindow = null;
            }

            s_traderWindow = window;
        }
    }

    /// <summary>
    /// Called when a trader window closes
    /// </summary>
    /// <param name="window">The trader window that closed</param>
    internal static void OnTraderWindowClosing(XUiC_TraderWindowGroup window)
    {
        lock (s_traderWindowLock)
        {
            if (s_traderWindow == window)
            {
                s_traderWindow = null;
            }
            else if (s_traderWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close trader window that doesn't match tracked instance.");
            }
        }
    }

    #endregion

    #region Quest Turn-In Window

    /// <summary>
    /// Gets whether the quest turn-in window is currently open
    /// </summary>
    /// <returns>True if the quest turn-in window is open, false otherwise</returns>
    public static bool IsQuestTurnInWindowOpen()
    {
        lock (s_questTurnInWindowLock)
        {
            return s_questTurnInWindow != null;
        }
    }

    /// <summary>
    /// Gets the currently active quest turn-in window instance
    /// </summary>
    /// <returns>The active quest turn-in window, or null if none is open</returns>
    public static XUiC_QuestTurnInWindowGroup GetActiveQuestTurnInWindow()
    {
        lock (s_questTurnInWindowLock)
        {
            return s_questTurnInWindow;
        }
    }

    /// <summary>
    /// Called when a quest turn-in window opens
    /// </summary>
    /// <param name="window">The quest turn-in window that opened</param>
    internal static void OnQuestTurnInWindowOpening(XUiC_QuestTurnInWindowGroup window)
    {
        lock (s_questTurnInWindowLock)
        {
            if (s_questTurnInWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Quest turn-in window opened while another was already tracked. Resetting state. Previous: {s_questTurnInWindow?.GetType().Name}, New: {window?.GetType().Name}");
                s_questTurnInWindow = null;
            }

            s_questTurnInWindow = window;
        }
    }

    /// <summary>
    /// Called when a quest turn-in window closes
    /// </summary>
    /// <param name="window">The quest turn-in window that closed</param>
    internal static void OnQuestTurnInWindowClosing(XUiC_QuestTurnInWindowGroup window)
    {
        lock (s_questTurnInWindowLock)
        {
            if (s_questTurnInWindow == window)
            {
                s_questTurnInWindow = null;
            }
            else if (s_questTurnInWindow != null)
            {
                ModLogger.Warning($"[WindowStateManager] Attempted to close quest turn-in window that doesn't match tracked instance.");
            }
        }
    }

    #endregion
}
