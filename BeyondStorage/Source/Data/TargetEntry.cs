namespace BeyondStorage.Data;

/// <summary>
/// A single registered push/pull target held by <see cref="TargetDistanceStore"/>.
/// Pairs the target with its distance from the player and its pre-built slot map views.
/// </summary>
internal sealed class TargetEntry
{
    internal IStorageTarget Storage => field;
    internal float Distance => field;
    internal SlotMaps All => field;
    internal SlotMaps Pushable => field;
    internal SlotMaps Loadout => field;
    internal SlotMaps Empty => field;

    internal TargetEntry(IStorageTarget storage, float distance, SlotMaps all, SlotMaps pushable, SlotMaps loadout, SlotMaps empty)
    {
        Storage = storage;
        Distance = distance;

        All = all;
        Pushable = pushable;
        Loadout = loadout;
        Empty = empty;
    }
}
