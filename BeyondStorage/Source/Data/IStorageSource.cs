using System;

namespace BeyondStorage.Data;

public interface IStorageSource : IEquatable<IStorageSource>
{
    /// <summary>
    /// Returns all slots without any filtering, including empty slots.
    /// Classification of consumable, pushable, and empty slots is the responsibility
    /// of <see cref="StorageSourceItemDataStore"/>, not the class implementing this interface.
    /// </summary>
    ItemStack[] GetAllItemStacks();
    Type GetSourceType();
    void MarkModified();

    /// <summary>
    /// Invoked once at the end of a multi-target push or pull so the source can perform
    /// any cross-cutting work that has to happen after the batch is committed
    /// (e.g. the dedicated-server vehicle bag broadcast).
    /// </summary>
    void FinaliseBulkChange();
}
