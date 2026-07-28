using System.Collections.Generic;
using BeyondStorage.Data;

namespace BeyondStorage.Storage.TransferTargets;

internal class SmartOnMissionTargetAdapter : ITransferAdapter
{
    string ITransferAdapter.GetAdapterName()
        => nameof(SmartOnMissionTargetAdapter);

    IReadOnlyList<StorageTargetAdapter> ITransferAdapter.GetAdapters(StorageContext context)
        => context.GetClosestStorageAdapters(StorageAdapterPolicy.SmartOnMissionPushAdapters, ItemScope.AllItems);
}
