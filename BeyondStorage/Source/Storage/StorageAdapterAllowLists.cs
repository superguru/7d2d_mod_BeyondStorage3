using System;
using System.Collections.Generic;

namespace BeyondStorage.Storage;

/// <summary>
/// Provides immutable allowed source lists for each storage operation type.
/// Each list is built once at first access and never changes.
/// </summary>
internal static class StorageAdapterAllowLists
{

    /// <summary>Gets the allowed source types for smart push operations.</summary>
    internal static AllowedAdapterTypeList SmartPushAdapters { get; } = BuildSmartPushAdapterList();

    internal static AllowedAdapterTypeList SmartPushMobileAdapters { get; } = BuildSmartPushMobileAdapterList();

    /// <summary>Gets the allowed source types for smart loadout pull operations.</summary>
    internal static AllowedAdapterTypeList SmartLoadoutPullAdapters { get; } = BuildSmartLoadoutPullAdapterList();

    private static AllowedAdapterTypeList BuildSmartPushAdapterList()
    {
        var types = new List<Type>
        {
            typeof(ITileEntityLootable),
        };

        return new AllowedAdapterTypeList(types);
    }

    private static AllowedAdapterTypeList BuildSmartPushMobileAdapterList()
    {
        var types = new List<Type>
        {
            typeof(EntityDrone),
            typeof(EntityVehicle),
        };

        return new AllowedAdapterTypeList(types);
    }

    private static AllowedAdapterTypeList BuildSmartLoadoutPullAdapterList()
    {
        var types = new List<Type>
        {
            typeof(TileEntityWorkstation),
            typeof(ITileEntityLootable),
        };

        return new AllowedAdapterTypeList(types);
    }
}