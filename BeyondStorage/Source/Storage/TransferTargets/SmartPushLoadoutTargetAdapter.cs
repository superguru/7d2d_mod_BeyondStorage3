using System.Collections.Generic;
using BeyondStorage.Data;

namespace BeyondStorage.Storage.TransferTargets;

internal class SmartPushLoadoutTargetAdapter : ITransferAdapter
{
    string ITransferAdapter.GetAdapterName()
        => nameof(SmartPushLoadoutTargetAdapter);

    IReadOnlyList<StorageTargetAdapter> ITransferAdapter.GetAdapters(StorageContext context)
        => context.GetClosestStorageAdapters(StorageAdapterAllowLists.SmartPushMobileAdapters, ItemScope.Loadout);
}
