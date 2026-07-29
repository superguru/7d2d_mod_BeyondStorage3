using System;

namespace BeyondStorage.Storage.SmartSorting;

[Flags]
public enum SmartPushScope
{
    Nowhere = 0,
    LoadoutsOnly = 1,
    StoragesOnly = 2,
    OverflowToEmpty = 4,

    LoadoutsThenStorages = LoadoutsOnly | StoragesOnly,
    LoadoutsThenStoragesThenOverflowToEmpty = LoadoutsOnly | StoragesOnly | OverflowToEmpty,
}