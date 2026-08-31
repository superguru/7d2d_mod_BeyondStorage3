using System.Collections.Generic;

namespace BeyondStorage.Storage.TransferTargets;

internal static class TransferAdapterServer
{
    internal static IReadOnlyList<ITransferAdapter> GetSmartPushTargetAdapters()
    {
        return [
            new SmartPushStationaryTargetAdapter(),
            new SmartPushMobileTargetAdapter(),
        ];
    }

    internal static IReadOnlyList<ITransferAdapter> GetSmartPushTargetOverflowAdapters()
    {
        return [
            new SmartPushStationaryTargetOverflowAdapter(),
            new SmartPushMobileTargetOverflowAdapter(),
        ];
    }

    internal static IReadOnlyList<ITransferAdapter> SmartPushLoadoutTargetAdapter()
    {
        return [
            new SmartPushLoadoutTargetAdapter(),
        ];
    }

    internal static IReadOnlyList<ITransferAdapter> GetSmartPullSourceAdapters()
    {
        return [
            new SmartPullLoadoutSourceAdapter(),
        ];
    }
}
