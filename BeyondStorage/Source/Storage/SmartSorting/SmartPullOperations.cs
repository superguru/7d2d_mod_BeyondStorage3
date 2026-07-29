using BeyondStorage.Data;
using BeyondStorage.Game.UI;
using BeyondStorage.Infrastructure;
using BeyondStorage.Source.Storage.TransferTargets;

namespace BeyondStorage.Storage.SmartSorting;

/// <summary>
/// UI-facing dispatcher for smart pull operations.
/// Resolves the source/target adapters for each operation type and delegates
/// all transfer logic to <see cref="ItemTransferEngine"/>.
/// </summary>
public class SmartPullOperations
{
    public const string MSG_SMART_PULL_LOADOUT_RESULT = "msgBeyondSmartPullLoadout_Result";

    private static void HandlePullFromStorages<L>(string methodName, StorageContext context, StorageSourceAdapter<L> loadout) where L : class
    {
        var sources = TransferAdapterServer.GetSmartPullSourceAdapters();

        ItemTransferEngine.PerformSmartLoadoutPull(methodName, context, loadout, sources);
    }

    public static void SmartPullToPlayerLoadout(XUiController _sender, int _mouseButton)
    {
        const string d_MethodName = nameof(SmartPullToPlayerLoadout);

#if DEBUG
        //ModLogger.DebugLog($"{methodName}: Starting smart pull to player loadout");
#endif

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out StorageContext context))
        {
            ModLogger.DebugLog($"{d_MethodName}: Validation failed, returning");
            return;
        }

        var loadout = StorageSourceAdapterFactory.CreatePlayerBackpackSourceAdapter(context, context.Player);
        HandlePullFromStorages(d_MethodName, context, loadout);
    }

    public static void SmartPullToVehicleOrDroneLoadout(XUiController _sender, int _mouseButton)
    {
        const string d_MethodName = nameof(SmartPullToVehicleOrDroneLoadout);

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
            //ModLogger.DebugLog($"{methodName}: Starting smart pull to drone loadout");
#endif

            var loadout = StorageSourceAdapterFactory.CreateDroneStorageSourceAdapter(context, drone);
            HandlePullFromStorages(d_MethodName, context, loadout);

            return;
        }

#if DEBUG
        //ModLogger.DebugLog($"{methodName}: No drone found, checking for vehicle");
#endif

        var vehicle = WindowStateManager.GetOpenWindowVehicle();
        if (vehicle != null)
        {
#if DEBUG
            //ModLogger.DebugLog($"{methodName}: Starting smart pull to vehicle loadout");
#endif
            var loadout = StorageSourceAdapterFactory.CreateVehicleStorageSourceAdapter(context, vehicle);
            HandlePullFromStorages(d_MethodName, context, loadout);

            return;
        }

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Nothing eligible found");
#endif
    }
}
