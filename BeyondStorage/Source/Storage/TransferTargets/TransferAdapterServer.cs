using System.Collections.Generic;
using BeyondStorage.Storage.TransferTargets;

namespace BeyondStorage.Source.Storage.TransferTargets;

internal static class TransferAdapterServer
{
    internal static IReadOnlyList<ITransferAdapter> GetSmartPushTargetAdapters()
    {
        return [
            new SmartPushTargetAdapter(),
            new SmartOnMissionTargetAdapter(),
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
