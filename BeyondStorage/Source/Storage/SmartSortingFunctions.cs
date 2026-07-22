using System.Collections.Generic;
using BeyondStorage.Data;
using BeyondStorage.Game.UI;
using BeyondStorage.Infrastructure;

namespace BeyondStorage.Storage;

/// <summary>
/// UI-facing dispatcher for smart push and pull operations.
/// Resolves the source/target adapters for each operation type and delegates
/// all transfer logic to <see cref="ItemTransferEngine"/>.
/// </summary>
public class SmartSortingFunctions
{
    public const string MSG_SMART_PULL_LOADOUT_RESULT = "msgBeyondSmartPullLoadout_Result";
    public const string MSG_SMART_PUSH_RESULT = "msgBeyondSmartPush_Result";

    private static IReadOnlyList<StorageTargetAdapter> GetSmartPushTargets(StorageContext context)
        => context.GetClosestStorageSources(StorageSourcePolicy.SmartPushSources, ItemScope.AllItems);

    private static IReadOnlyList<StorageTargetAdapter> GetSmartOnMissionPushTargets(StorageContext context)
        => context.GetClosestStorageSources(StorageSourcePolicy.SmartOnMissionPushSources, ItemScope.AllItems);

    private static IReadOnlyList<StorageTargetAdapter> GetSmartLoadoutPullSources(StorageContext context)
        => context.GetClosestStorageSources(StorageSourcePolicy.SmartLoadoutPullSources, ItemScope.PushableItems);

    public static void SmartCollectorPush()
    {
        const string d_MethodName = nameof(SmartCollectorPush);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting smart push from collector");
#endif

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return;
        }

        var collector = WindowStateManager.GetOpenCollectorTileEntity();
        if (collector == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: No open collector found, returning");
            return;
        }

        var source = StorageSourceAdapterFactory.CreateCollectorStorageSourceAdapter(context, collector);
        var targets = GetSmartPushTargets(context);

        ItemTransferEngine.PerformSmartPush(d_MethodName, context, source, targets, GetSmartOnMissionPushTargets);
    }

    public static void SmartLootWindowPush()
    {
        const string d_MethodName = nameof(SmartLootWindowPush);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting smart push from loot window");
#endif

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return;
        }

        var lootable = WindowStateManager.GetOpenWindowLootable();
        if (lootable == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: No open loot window found, returning");
            return;
        }

        SmartPushFromPlayerCreatedStorage(context, lootable);
    }

    private static void SmartPushFromPlayerCreatedStorage(StorageContext context, ITileEntityLootable lootable)
    {
        const string d_MethodName = nameof(SmartPushFromPlayerCreatedStorage);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting smart push from player created storage");
#endif
        var source = StorageSourceAdapterFactory.CreateLootableStorageSourceAdapter(context, lootable);
        var targets = GetSmartPushTargets(context);

        ItemTransferEngine.PerformSmartPush(d_MethodName, context, source, targets, GetSmartOnMissionPushTargets);
    }

    public static void SmartDroneInventoryLoadoutPull(StorageContext context, EntityDrone drone)
    {
        const string d_MethodName = nameof(SmartDroneInventoryLoadoutPull);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting smart pull to drone storage");
#endif

        var loadout = StorageSourceAdapterFactory.CreateDroneStorageSourceAdapter(context, drone);
        var sources = GetSmartLoadoutPullSources(context);

        ItemTransferEngine.PerformSmartLoadoutPull(d_MethodName, context, loadout, sources);
    }

    private static void SmartPushFromDroneStorage(StorageContext context, EntityDrone drone)
    {
        const string d_MethodName = nameof(SmartPushFromDroneStorage);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting smart push from drone storage");
#endif
        var source = StorageSourceAdapterFactory.CreateDroneStorageSourceAdapter(context, drone);
        var targets = GetSmartPushTargets(context);

        ItemTransferEngine.PerformSmartPush(d_MethodName, context, source, targets, GetSmartOnMissionPushTargets);
    }

    public static void SmartPlayerInventoryLoadoutPull()
    {
        const string d_MethodName = nameof(SmartPlayerInventoryLoadoutPull);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting smart pull to player inventory");
#endif

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return;
        }

        var loadout = StorageSourceAdapterFactory.CreatePlayerBackpackSourceAdapter(context, context.Player);
        var sources = GetSmartLoadoutPullSources(context);

        ItemTransferEngine.PerformSmartLoadoutPull(d_MethodName, context, loadout, sources);
    }

    public static void SmartPlayerInventoryPush()
    {
        const string d_MethodName = nameof(SmartPlayerInventoryPush);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting smart push from player inventory");
#endif

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return;
        }

        var source = StorageSourceAdapterFactory.CreatePlayerBackpackSourceAdapter(context, context.Player);
        var targets = GetSmartPushTargets(context);

        ItemTransferEngine.PerformSmartPush(d_MethodName, context, source, targets, GetSmartOnMissionPushTargets);
    }

    public static void SmartVehicleLoadoutPull()
    {
        const string d_MethodName = nameof(SmartVehicleLoadoutPull);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting smart pull to vehicle/drone");
#endif

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return;
        }

        var drone = WindowStateManager.GetOpenWindowDrone();
        if (drone != null)
        {
            SmartDroneInventoryLoadoutPull(context, drone);
            return;
        }

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: No open drone found for pull, going to try vehicle3");
#endif

        var vehicle = WindowStateManager.GetOpenWindowVehicle();
        if (vehicle == null)
        {
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: No open vehicle3 found for pull, returning");
#endif
            return;
        }

        var loadout = StorageSourceAdapterFactory.CreateVehicleStorageSourceAdapter(context, vehicle);
        var sources = GetSmartLoadoutPullSources(context);

        ItemTransferEngine.PerformSmartLoadoutPull(d_MethodName, context, loadout, sources);
    }

    public static void SmartVehiclePush()
    {
        const string d_MethodName = nameof(SmartVehiclePush);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting smart push from vehicle/drone");
#endif

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return;
        }

        // Drone storage is opened via the loot window — handle it before the generic lootable path
        var drone = WindowStateManager.GetOpenWindowDrone();
        if (drone != null)
        {
            SmartPushFromDroneStorage(context, drone);
            return;
        }

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: No open drone found for push, going to try vehicle3");
#endif

        var vehicle = WindowStateManager.GetOpenWindowVehicle();
        if (vehicle == null)
        {
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: No open vehicle3 found for push, returning");
#endif
            return;
        }

        var source = StorageSourceAdapterFactory.CreateVehicleStorageSourceAdapter(context, vehicle);
        var targets = GetSmartPushTargets(context);

        ItemTransferEngine.PerformSmartPush(d_MethodName, context, source, targets, GetSmartOnMissionPushTargets);
    }

    public static void SmartDroppedLootPush()
    {
        const string d_MethodName = nameof(SmartDroppedLootPush);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting smart push from dropped loot");
#endif

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return;
        }

        var container = WindowStateManager.GetOpenWindowDroppedLoot();
        if (container == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: No open dropped loot window found, returning");
            return;
        }

        var source = StorageSourceAdapterFactory.CreateDroppedLootSourceAdapter(context, container);
        var targets = GetSmartPushTargets(context);

        ItemTransferEngine.PerformSmartPush(d_MethodName, context, source, targets, GetSmartOnMissionPushTargets);
    }

    public static void SmartWorkstationOutputPush()
    {
        const string d_MethodName = nameof(SmartWorkstationOutputPush);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting smart push from workstation");
#endif

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return;
        }

        var workstation = WindowStateManager.GetOpenWorkstationTileEntity();
        if (workstation == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: No open workstation found, returning");
            return;
        }

        var source = StorageSourceAdapterFactory.CreateWorkstationStorageSourceAdapter(context, workstation);
        var targets = GetSmartPushTargets(context);

        ItemTransferEngine.PerformSmartPush(d_MethodName, context, source, targets, GetSmartOnMissionPushTargets);
    }
}
