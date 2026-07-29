using BeyondStorage.Data;
using BeyondStorage.Game.UI;
using BeyondStorage.Infrastructure;
using BeyondStorage.Source.Storage.TransferTargets;

namespace BeyondStorage.Storage.SmartSorting;

/// <summary>
/// UI-facing dispatcher for smart push operations.
/// Resolves the source/target adapters for each operation type and delegates
/// all transfer logic to <see cref="ItemTransferEngine"/>.
/// </summary>
public class SmartPushOperations
{
    public const string MSG_SMART_PUSH_RESULT = "msgBeyondSmartPush_Result";

    private static void HandlePushToLoadouts<S>(string methodName, StorageContext context, StorageSourceAdapter<S> source) where S : class
    {
        var targets = TransferAdapterServer.SmartPushLoadoutTargetAdapter();

        ItemTransferEngine.PerformSmartPush($"{methodName}.L", context, source, targets);
    }

    private static void HandlePushToStorages<S>(string methodName, StorageContext context, StorageSourceAdapter<S> source) where S : class
    {
        var targets = TransferAdapterServer.GetSmartPushTargetAdapters();

        ItemTransferEngine.PerformSmartPush($"{methodName}.S", context, source, targets);
    }

    private static void HandlePushToMulti<S>(string methodName, StorageContext context, StorageSourceAdapter<S> source) where S : class
    {
        HandlePushToLoadouts(methodName, context, source);
        HandlePushToStorages(methodName, context, source);
    }

    public static void SmartPushFromCollector()
    {
        const string d_MethodName = nameof(SmartPushFromCollector);

#if DEBUG
        //ModLogger.DebugLog($"{methodName}: Starting");
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
        HandlePushToMulti(d_MethodName, context, source);
    }

    public static void SmartPushFromLootable()
    {
        const string d_MethodName = nameof(SmartPushFromLootable);

#if DEBUG
        //ModLogger.DebugLog($"{methodName}: Starting");
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
        HandlePushToMulti(d_MethodName, context, source);
    }

    public static void SmartPushFromPlayerBackpack()
    {
        const string d_MethodName = nameof(SmartPushFromPlayerBackpack);

#if DEBUG
        //ModLogger.DebugLog($"{methodName}: Starting");
#endif

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return;
        }

        var source = StorageSourceAdapterFactory.CreatePlayerBackpackSourceAdapter(context, context.Player);
        HandlePushToMulti(d_MethodName, context, source);
    }
    public static void SmartPushFromVehicleOrDrone()
    {
        const string d_MethodName = nameof(SmartPushFromVehicleOrDrone);

#if DEBUG
        //ModLogger.DebugLog($"{methodName}: Starting");
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
            //ModLogger.DebugLog($"{methodName}: Starting smart push from drone");
#endif
            var source = StorageSourceAdapterFactory.CreateDroneStorageSourceAdapter(context, drone);
            HandlePushToStorages(d_MethodName, context, source);

            return;
        }

#if DEBUG
        //ModLogger.DebugLog($"{methodName}: No drone found, checking for vehicle");
#endif

        var vehicle = WindowStateManager.GetOpenWindowVehicle();
        if (vehicle != null)
        {
#if DEBUG
            //ModLogger.DebugLog($"{methodName}: Starting smart push from vehicle");
#endif
            var source = StorageSourceAdapterFactory.CreateVehicleStorageSourceAdapter(context, vehicle);
            HandlePushToStorages(d_MethodName, context, source);

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
        //ModLogger.DebugLog($"{methodName}: Starting");
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
        HandlePushToMulti(d_MethodName, context, source);
    }

    public static void SmartPushFromWorkstation()
    {
        const string d_MethodName = nameof(SmartPushFromWorkstation);

#if DEBUG
        //ModLogger.DebugLog($"{methodName}: Starting");
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
        HandlePushToMulti(d_MethodName, context, source);
    }
}
