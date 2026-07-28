namespace BeyondStorage.Data;

/// <summary>
/// A single registered push/pull target held by <see cref="TargetDistanceStore"/>.
/// Pairs the target with its distance from the player and its pre-built slot map views.
/// </summary>
internal sealed class TargetEntry
{
    internal IStorageTarget Storage { get; }
    internal float Distance { get; }
    internal SlotMaps AllItems { get; }
    internal SlotMaps Pushable { get; }
    internal SlotMaps Loadout { get; }

    internal TargetEntry(IStorageTarget storage, float distance, SlotMaps allItems, SlotMaps pushable, SlotMaps loadout)
    {
        Storage = storage;
        Distance = distance;
        AllItems = allItems;
        Pushable = pushable;
        Loadout = loadout;
    }
}
