using System.Linq;
using BeyondStorage.Game.UI;
using BeyondStorage.Infrastructure;
using BeyondStorage.Multiplayer;

namespace BeyondStorage.Entities;

public static class EntityHandler
{
    public static string GetEntityName(Entity entity)
    {
#if DEBUG
        const string d_MethodName = nameof(GetEntityName);
#endif
        string name = "Unnamed Entity";

        if (entity == null)
        {
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: entity is null, returning default name");
#endif
            return name;
        }

        // Check cache first
        if (EntityNameCache.TryGetName(entity, out string cachedName))
        {
            return cachedName;
        }

        var localisedName = entity.LocalizedEntityName;
        if (!string.IsNullOrEmpty(localisedName))
        {
            name = localisedName;
        }

        EntityNameCache.CacheName(entity, name);
        return name;
    }

    public static string GetPlayerName(EntityPlayerLocal entity)
    {
        string name = "Unnamed Player Lootable";

        var cachedPlayerName = entity?.cachedPlayerName;
        if (cachedPlayerName != null)
        {
            var displayname = cachedPlayerName.DisplayName;
            if (!string.IsNullOrEmpty(displayname))
            {
                //name = $"[007F0E]{displayname}[-]";  // decorated version with green color
                name = displayname;
            }
        }

        return name;
    }

    public static string GetPlayerBackpackName()
    {
        const string d_MethodName = nameof(GetPlayerBackpackName);
        string name = "Backpack";

        return GameTools.GetLocalisedValue(d_MethodName, name);
    }

    /// <summary>
    /// Gets all item stacks from an entity's bag without any filtering.
    /// </summary>
    /// <param name="entity">The entity to get items from</param>
    /// <returns>Array of all ItemStack objects in the entity's bag, or an empty array if the bag is null or empty</returns>
    public static ItemStack[] GetAllSlotItems(Entity entity)
    {
        var itemGrid = entity?.bag?.ItemGrid;
        if (itemGrid == null || itemGrid.Length == 0)
        {
            return [];
        }

        var items = new ItemStack[itemGrid.Length];
        for (var i = 0; i < itemGrid.Length; i++)
        {
            items[i] = itemGrid[i];
        }
        return items;
    }

    public static ItemStack[] GetPlayerToolbeltAllSlotItems(EntityPlayerLocal player)
    {
        var itemGrid = player.inventory?.ItemGrid;
        if (itemGrid == null || itemGrid.Length == 0)
        {
            return [];
        }

        var items = new ItemStack[itemGrid.Length];
        for (var i = 0; i < itemGrid.Length; i++)
        {
            items[i] = itemGrid[i];
        }
        return items;
    }

    /// <summary>
    /// Marks a player vehicle's inventory as modified, triggering UI updates for backpack and toolbelt.
    /// </summary>
    /// <param name="entity">The player vehicle whose inventory was modified</param>
    public static void MarkPlayerInventoryModified(EntityPlayerLocal entity)
    {
        const string d_MethodName = nameof(MarkPlayerInventoryModified);

        if (entity == null || entity.playerUI == null || entity.playerUI.xui == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: entity or player UI is null");
            return;
        }

        entity.playerUI.xui.PlayerInventory.dispatchBackpackItemsChanged();
        entity.playerUI.xui.PlayerInventory.dispatchToolbeltItemsChanged();
    }

    public static void MarkDroneStorageModified(EntityDrone drone)
    {
        const string d_MethodName = nameof(MarkDroneStorageModified);

        if (drone == null || drone.bag == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: entity or bag is null");
            return;
        }

        drone.OnBagModified();
        WindowStateManager.SetOpenWindowEntitiesModified();
    }

    /// <summary>
    /// Finalises a bulk item change on a drone's bag. Called once at the end of a
    /// multi-target smart push/pull so the drone can perform any cross-cutting work
    /// (e.g. the dedicated-server broadcast that prevents the stale-peer item
    /// duplication described in the bug report). Mirrors the vehicle fix.
    /// </summary>
    public static void FinaliseDroneBulkChange(EntityDrone drone)
    {
        const string d_MethodName = nameof(FinaliseDroneBulkChange);

        if (drone == null || drone.bag == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: entity or bag is null");
            return;
        }

        var connectionManager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (connectionManager == null)
        {
            return;
        }

        // Single player: the local bag IS the only bag, no peers to inform.
        if (WorldTools.IsSinglePlayer())
        {
            return;
        }

        int senderId = GameManager.Instance.World.GetPrimaryPlayerId();

        var package = NetPackageManager.GetPackage<NetPackageBeyondStorageDroneBagUpdate>()
                                       .Setup(drone, senderId);

        if (connectionManager.IsServer)
        {
            if (connectionManager.Clients == null || connectionManager.Clients.List.Count == 0)
            {
#if DEBUG
                ModLogger.DebugLog($"{d_MethodName}: Server with no connected clients, skipping broadcast for drone {drone.entityId}");
#endif
                return;
            }

#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: Broadcasting drone {drone.entityId} bag ({package.BagDataSizeBytes}B) from senderId {senderId} to {connectionManager.Clients.List.Count} client(s)");
#endif
            connectionManager.SendPackage(package, _onlyClientsAttachedToAnEntity: false);
        }
        else
        {
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: Sending drone {drone.entityId} bag ({package.BagDataSizeBytes}B) from senderId {senderId} to server");
#endif
            connectionManager.SendToServer(package);
        }
    }

    public static void MarkDroppedLootModified(EntityLootContainer container)
    {
        const string d_MethodName = nameof(MarkVehicleStorageModified);

        if (container == null || container.bag == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: entity or bag is null");
            return;
        }

        container.OnBagModified();
        WindowStateManager.SetOpenWindowEntitiesModified();
    }

    public static void MarkVehicleStorageModified(EntityVehicle vehicle)
    {
        const string d_MethodName = nameof(MarkVehicleStorageModified);

        if (vehicle == null || vehicle.bag == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: entity or bag is null");
            return;
        }

        vehicle.SetBagModified();
        WindowStateManager.SetOpenWindowEntitiesModified();
    }

    /// <summary>
    /// Finalises a bulk item change on a vehicle's bag. Called once at the end of a
    /// multi-target smart push/pull so the vehicle can perform any cross-cutting work
    /// (e.g. the dedicated-server broadcast that prevents the stale-peer item
    /// duplication described in the bug report).
    /// </summary>
    public static void FinaliseVehicleBulkChange(EntityVehicle vehicle)
    {
        const string d_MethodName = nameof(FinaliseVehicleBulkChange);

        if (vehicle == null || vehicle.bag == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: entity or bag is null");
            return;
        }

        var connectionManager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (connectionManager == null)
        {
            // Shutting down or not yet initialised — nothing to broadcast.
            return;
        }

        // Single player: the local bag IS the only bag, no peers to inform.
        if (WorldTools.IsSinglePlayer())
        {
            return;
        }

        // Pick the sender id from the local primary player (the player whose
        // smart push/pull just ran). On a listen-server host this is the host's
        // player; on a dedicated-server client this is the local player.
        int senderId = GameManager.Instance.World.GetPrimaryPlayerId();

        var package = NetPackageManager.GetPackage<NetPackageBeyondStorageVehicleBagUpdate>()
                                       .Setup(vehicle, senderId);

        if (connectionManager.IsServer)
        {
            // Listen-server host: the host already mutated vehicle.bag in place,
            // so its own state is correct. We only need to inform the connected
            // remote clients. Skip the broadcast entirely if there are none
            // (e.g. during the brief window after world load before any player
            // has connected).
            if (connectionManager.Clients == null || connectionManager.Clients.List.Count == 0)
            {
#if DEBUG
                ModLogger.DebugLog($"{d_MethodName}: Server with no connected clients, skipping broadcast for vehicle {vehicle.entityId}");
#endif
                return;
            }

#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: Broadcasting vehicle {vehicle.entityId} bag ({package.BagDataSizeBytes}B) from senderId {senderId} to {connectionManager.Clients.List.Count} client(s)");
#endif
            connectionManager.SendPackage(package, _onlyClientsAttachedToAnEntity: false);
        }
        else
        {
            // Dedicated-server client: the local bag was mutated but the server
            // (and therefore every other client) is still on the stale snapshot.
            // Hand the new snapshot to the server; its ProcessPackage will apply
            // it and rebroadcast to peers.
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: Sending vehicle {vehicle.entityId} bag ({package.BagDataSizeBytes}B) from senderId {senderId} to server");
#endif
            connectionManager.SendToServer(package);
        }
    }
}
