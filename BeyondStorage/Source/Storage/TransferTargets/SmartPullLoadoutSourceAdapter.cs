using System.Collections.Generic;
using BeyondStorage.Data;

namespace BeyondStorage.Storage.TransferTargets;

internal class SmartPullLoadoutSourceAdapter : ITransferAdapter
{
    string ITransferAdapter.GetAdapterName()
        => nameof(SmartPullLoadoutSourceAdapter);

    IReadOnlyList<StorageTargetAdapter> ITransferAdapter.GetAdapters(StorageContext context)
        => context.GetClosestStorageAdapters(StorageAdapterPolicy.SmartLoadoutPullAdapters, ItemScope.PushableItems);
}
