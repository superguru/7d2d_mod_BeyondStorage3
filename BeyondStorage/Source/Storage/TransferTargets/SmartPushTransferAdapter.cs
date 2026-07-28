using System.Collections.Generic;
using BeyondStorage.Data;

namespace BeyondStorage.Storage.TransferTargets;

internal class SmartPushTransferAdapter : ITransferAdapter
{
    string ITransferAdapter.GetAdapterName()
        => nameof(SmartPushTransferAdapter);

    IReadOnlyList<StorageTargetAdapter> ITransferAdapter.GetAdapters(StorageContext context)
        => context.GetClosestStorageAdapters(StorageAdapterPolicy.SmartPushAdapters, ItemScope.AllItems);
}
