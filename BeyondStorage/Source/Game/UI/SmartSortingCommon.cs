using BeyondStorage.Storage.SmartSorting;

namespace BeyondStorage.Game.UI;

/// <summary>
/// UI event bindings for smart sorting operations. Each method bridges the game's XUiController click system
/// to the underlying <see cref="SmartPushOperations"/> engine, delegating all transfer logic there.
/// </summary>
public class SmartSortingCommon
{
    // ── Push FROM sources into storage ───────────────────────────────────────────

    /// <summary>
    /// Push items from an open collector container into nearby storage units.
    /// </summary>
    public static void SmartPushFromCollector_EventHandler(XUiController _sender, int _mouseButton)
        => SmartPushOperations.SmartPushFromCollector(_sender, _mouseButton);

    /// <summary>
    /// Push items from a lootable container window into nearby storage units.
    /// </summary>
    public static void SmartPushFromLootable_EventHandler(XUiController _sender, int _mouseButton)
        => SmartPushOperations.SmartPushFromLootable(_sender, _mouseButton);

    /// <summary>
    /// Push items from the player's backpack into nearby storage units.
    /// </summary>
    public static void SmartPushFromPlayerBackpack_EventHandler(XUiController _sender, int _mouseButton)
        => SmartPushOperations.SmartPushFromPlayerBackpack(_sender, _mouseButton);

    /// <summary>
    /// Push items from a drone or vehicle container into nearby storage units.
    /// </summary>
    public static void SmartPushFromVehicleOrDrone_EventHandler(XUiController _sender, int _mouseButton)
        => SmartPushOperations.SmartPushFromVehicleOrDrone(_sender, _mouseButton);

    /// <summary>
    /// Push items from dropped loot on the ground into nearby storage units.
    /// </summary>
    public static void SmartPushFromDroppedLoot_EventHandler(XUiController _sender, int _mouseButton)
        => SmartPushOperations.SmartPushFromDroppedLoot(_sender, _mouseButton);

    /// <summary>
    /// Push items from an open workstation into nearby storage units.
    /// </summary>
    public static void SmartPushFromWorkstation_EventHandler(XUiController _sender, int _mouseButton)
        => SmartPushOperations.SmartPushFromWorkstation(_sender, _mouseButton);

    // ── Pull TO targets from sources ─────────────────────────────────────────────

    /// <summary>
    /// Pull pushable items into the player's backpack from nearby storage units.
    /// </summary>
    public static void SmartPullToPlayerLoadout_EventHandler(XUiController _sender, int _mouseButton)
        => SmartPullOperations.SmartPullToPlayerLoadout(_sender, _mouseButton);

    /// <summary>
    /// Pull pushable items into a drone or vehicle container from nearby storage units.
    /// </summary>
    public static void SmartPullToDroneLoadout_EventHandler(XUiController _sender, int _mouseButton)
        => SmartPullOperations.SmartPullToVehicleOrDroneLoadout(_sender, _mouseButton);

    /// <summary>
    /// Pull pushable items into a vehicle or drone container from nearby storage units.
    /// </summary>
    public static void SmartPullToVehicleLoadout_EventHandler(XUiController _sender, int _mouseButton)
        => SmartPullOperations.SmartPullToVehicleOrDroneLoadout(_sender, _mouseButton);
}