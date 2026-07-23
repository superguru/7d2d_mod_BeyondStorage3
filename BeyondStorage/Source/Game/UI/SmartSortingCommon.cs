using BeyondStorage.Storage;

namespace BeyondStorage.Game.UI;

public class SmartSortingCommon
{
    public static void SmartPushFromCollector_EventHandler(XUiController _sender, int _mouseButton)
    {
        SmartSortingFunctions.SmartPushFromCollector();
    }

    public static void SmartPullToPlayerLoadout_EventHandler(XUiController _sender, int _mouseButton)
    {
        SmartSortingFunctions.SmartPullToPlayerLoadout();
    }

    public static void SmartPushFromPlayerBackpack_EventHandler(XUiController _sender, int _mouseButton)
    {
        SmartSortingFunctions.SmartPushFromPlayerBackpack();
    }

    public static void SmartPullToDroneLoadout_EventHandler(XUiController _sender, int _mouseButton)
    {
        SmartSortingFunctions.SmartPullToVehicleOrDroneLoadout();
    }

    public static void SmartPushFromLootable_EventHandler(XUiController _sender, int _mouseButton)
    {
        SmartSortingFunctions.SmartPushFromLootable();
    }

    public static void SmartPullToVehicleLoadout_EventHandler(XUiController _sender, int _mouseButton)
    {
        SmartSortingFunctions.SmartPullToVehicleOrDroneLoadout();
    }

    public static void SmartPushFromVehicleOrDrone_EventHandler(XUiController _sender, int _mouseButton)
    {
        SmartSortingFunctions.SmartPushFromVehicleOrDrone();
    }

    public static void SmartPushFromDroppedLoot_EventHandler(XUiController _sender, int _mouseButton)
    {
        SmartSortingFunctions.SmartPushFromDroppedLoot();
    }

    public static void SmartPushFromWorkstation_EventHandler(XUiController _sender, int _mouseButton)
    {
        SmartSortingFunctions.SmartPushFromWorkstation();
    }
}