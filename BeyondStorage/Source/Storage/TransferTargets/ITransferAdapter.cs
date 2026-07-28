using System.Collections.Generic;
using BeyondStorage.Data;

namespace BeyondStorage.Storage.TransferTargets;

internal interface ITransferAdapter
{
    string GetAdapterName();
    IReadOnlyList<StorageTargetAdapter> GetAdapters(StorageContext context);
}
