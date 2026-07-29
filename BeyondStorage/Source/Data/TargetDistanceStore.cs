using System.Collections.Generic;
using BeyondStorage.Infrastructure;
using BeyondStorage.Storage;

namespace BeyondStorage.Data;

/// <summary>
/// Stores (storage, distance) pairs with on-demand distance sorting.
/// Accepts any <see cref="IStorageTarget"/>, allowing mixed storage source types.
/// Slot maps are pre-built at registration and cloned per operation at query time.
/// Callers are responsible for ensuring each StorageSource is registered at most once.
/// </summary>
internal sealed class TargetDistanceStore
{
    private readonly List<TargetEntry> _entries = [];

    internal bool IsSorted { get; private set; } = true;

    internal void Add(IStorageTarget storage, float distance, SlotMaps all, SlotMaps pushable, SlotMaps loadout, SlotMaps empty)
    {
        const string d_MethodName = nameof(Add);

        if (storage == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: Null storage supplied");
            return;
        }

        _entries.Add(new TargetEntry(storage, distance, all, pushable, loadout, empty));
        IsSorted = false;
    }

    /// <summary>
    /// Sorts entries by distance ascending. No-op if already sorted.
    /// </summary>
    internal void Sort()
    {
        if (IsSorted)
        {
            return;
        }

        _entries.Sort(static (a, b) => a.Distance.CompareTo(b.Distance));
        IsSorted = true;
    }

    internal void Clear()
    {
        _entries.Clear();
        IsSorted = true;
    }

    internal IReadOnlyList<StorageTargetAdapter> GetClosestStorageSources(AllowedAdapterTypeList allowedAdapterTypes, ItemScope scope)
    {
        Sort();

        var result = new List<StorageTargetAdapter>(_entries.Count);
        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            if (!allowedAdapterTypes.IsAllowedSource(entry.Storage.GetSourceType()))
            {
                continue;
            }
            SlotMaps maps = SelectEntryByScope(entry, scope);
            result.Add(new StorageTargetAdapter(entry.Storage, entry.Distance, maps, anyItemTypeEligibleForEmptySlots: scope == ItemScope.Empty));
        }

        return result;
    }

    private static SlotMaps SelectEntryByScope(TargetEntry entry, ItemScope scope)
    {
        // Clone gives each operation its own mutable copy for ReclassifySlot
        var maps = scope switch
        {
            ItemScope.All => entry.All,
            ItemScope.Loadout => entry.Loadout,
            ItemScope.Pushable => entry.Pushable,
            ItemScope.Empty => entry.Empty,
            _ => null,
        };

        if (maps == null)
        {
            ModLogger.DebugLog($"Cannot select slot map from scope {scope}. Changed to empty slot map as mitigation.");
            maps = new();
        }

        return maps.Clone();
    }
}
