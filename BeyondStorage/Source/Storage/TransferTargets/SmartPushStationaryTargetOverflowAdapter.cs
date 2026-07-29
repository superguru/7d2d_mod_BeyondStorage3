using System.Collections.Generic;
using BeyondStorage.Data;

namespace BeyondStorage.Storage.TransferTargets;

internal class SmartPushStationaryTargetOverflowAdapter : ITransferAdapter
{
    string ITransferAdapter.GetAdapterName()
        => nameof(SmartPushStationaryTargetAdapter);

    IReadOnlyList<StorageTargetAdapter> ITransferAdapter.GetAdapters(StorageContext context)
        => context.GetClosestStorageAdapters(StorageAdapterAllowLists.SmartPushAdapters, ItemScope.Empty);
}
