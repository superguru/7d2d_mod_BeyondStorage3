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

    public static void SmartPushFromCollector()
    {
        const string d_MethodName = nameof(SmartPushFromCollector);

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

    public static void SmartPushFromLootable()
    {
        const string d_MethodName = nameof(SmartPushFromLootable);

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

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting smart push from '{lootable.lootListName}'");
#endif
        var source = StorageSourceAdapterFactory.CreateLootableStorageSourceAdapter(context, lootable);
        var targets = GetSmartPushTargets(context);

        ItemTransferEngine.PerformSmartPush(d_MethodName, context, source, targets, GetSmartOnMissionPushTargets);
    }

    public static void SmartPullToPlayerLoadout()
    {
        const string d_MethodName = nameof(SmartPullToPlayerLoadout);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting smart pull to player loadout");
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

    public static void SmartPushFromPlayerBackpack()
    {
        const string d_MethodName = nameof(SmartPushFromPlayerBackpack);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting");
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

    public static void SmartPullToVehicleOrDroneLoadout()
    {
        const string d_MethodName = nameof(SmartPullToVehicleOrDroneLoadout);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting");
#endif

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return;
        }

        var drone = WindowStateManager.GetOpenWindowDrone();
        if (drone != null)
        {
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: Starting smart pull to drone loadout");
#endif

            var loadout = StorageSourceAdapterFactory.CreateDroneStorageSourceAdapter(context, drone);
            var sources = GetSmartLoadoutPullSources(context);

            ItemTransferEngine.PerformSmartLoadoutPull(d_MethodName, context, loadout, sources);
            return;
        }

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: No drone found, checking for vehicle");
#endif

        var vehicle = WindowStateManager.GetOpenWindowVehicle();
        if (vehicle != null)
        {
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: Starting smart pull to vehicle loadout");
#endif
            var loadout = StorageSourceAdapterFactory.CreateVehicleStorageSourceAdapter(context, vehicle);
            var sources = GetSmartLoadoutPullSources(context);

            ItemTransferEngine.PerformSmartLoadoutPull(d_MethodName, context, loadout, sources);

            return;
        }

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Nothing eligible found");
#endif
    }

    public static void SmartPushFromVehicleOrDrone()
    {
        const string d_MethodName = nameof(SmartPushFromVehicleOrDrone);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting");
#endif

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return;
        }

        var drone = WindowStateManager.GetOpenWindowDrone();
        if (drone != null)
        {
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: Starting smart push from drone");
#endif
            var source = StorageSourceAdapterFactory.CreateDroneStorageSourceAdapter(context, drone);
            var targets = GetSmartPushTargets(context);

            ItemTransferEngine.PerformSmartPush(d_MethodName, context, source, targets, GetSmartOnMissionPushTargets);

            return;
        }

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: No drone found, checking for vehicle");
#endif

        var vehicle = WindowStateManager.GetOpenWindowVehicle();
        if (vehicle != null)
        {
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: Starting smart push from vehicle");
#endif
            var source = StorageSourceAdapterFactory.CreateVehicleStorageSourceAdapter(context, vehicle);
            var targets = GetSmartPushTargets(context);

            ItemTransferEngine.PerformSmartPush(d_MethodName, context, source, targets, GetSmartOnMissionPushTargets);

            return;
        }

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Nothing eligible found");
#endif
    }

    public static void SmartPushFromDroppedLoot()
    {
        const string d_MethodName = nameof(SmartPushFromDroppedLoot);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting");
#endif

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return;
        }

        var container = WindowStateManager.GetOpenWindowDroppedLoot();
        if (container == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: No open dropped loot found, returning");
            return;
        }

        var source = StorageSourceAdapterFactory.CreateDroppedLootSourceAdapter(context, container);
        var targets = GetSmartPushTargets(context);

        ItemTransferEngine.PerformSmartPush(d_MethodName, context, source, targets, GetSmartOnMissionPushTargets);
    }

    public static void SmartPushFromWorkstation()
    {
        const string d_MethodName = nameof(SmartPushFromWorkstation);

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Starting");
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
