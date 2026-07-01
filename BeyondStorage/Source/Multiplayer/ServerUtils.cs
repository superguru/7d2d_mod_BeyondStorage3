using System.Collections.Generic;
using BeyondStorage.Configuration;
using BeyondStorage.Entities;
using BeyondStorage.Infrastructure;

using static ModEvents;

namespace BeyondStorage.Multiplayer;

public static class ServerUtils
{
    public static bool HasServerConfig = false;

    public static void PlayerSpawnedInWorld(ref SPlayerSpawnedInWorldData data)
    {
        if (!ShouldProcessPlayerSpawn(data))
        {
            return;
        }

        ModLogger.DebugLog($"client {data.ClientInfo}; isLocalPlayer {data.IsLocalPlayer}; entityId {data.EntityId}; respawn type {data.RespawnType}; pos {data.Position}");

        SendCurrentLockedDict(data.ClientInfo);
        SendCurrentConsumeStates(data.ClientInfo);

        if (ModConfig.ServerSyncConfig())
        {
            data.ClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageBeyondStorageConfig>());
        }
    }

    private static bool ShouldProcessPlayerSpawn(SPlayerSpawnedInWorldData data)
    {
        var connectionManager = SingletonMonoBehaviour<ConnectionManager>.Instance;

        // Add null check
        if (connectionManager == null)
        {
            return false;
        }

        return connectionManager.IsServer &&
               !connectionManager.IsSinglePlayer &&
               data.ClientInfo != null;
    }

    private static void SendCurrentConsumeStates(ClientInfo client)
    {
        if (!IsValidDestination(client.entityId))
        {
            return;
        }
        BlockConsumeStates.SendConsumeStatesToClient(client);
    }

    private static void SendCurrentLockedDict(ClientInfo client)
    {
        if (TileEntityLocks.LockedTileEntities.IsEmpty || !IsValidDestination(client.entityId))
        {
            return;
        }

        var currentCopy = new Dictionary<Vector3i, int>(TileEntityLocks.LockedTileEntities);
        client.SendPackage(NetPackageManager.GetPackage<NetPackageLockedTEs>().Setup(currentCopy));

#if DEBUG
        ModLogger.DebugLog($"SendCurrentLockedDict to {client.entityId}");
#endif
    }

    public static bool IsTargetLockedClientCheck(ILockTarget target, ushort _channel = 0)
    {
        return LockManager.Instance.IsLockedByLocalPlayer(target, _channel);
    }

    private static bool IsValidDestination(int destinationId)
    {
#if DEBUG
        ModLogger.DebugLog($"PlayerSpawnedInWorld called with {destinationId}");
        if (destinationId == -1)
        {
            ModLogger.Error("PlayerSpawnedInWorld called without a valid entity id");
            return false;
        }

        if (!GameManager.IsDedicatedServer && destinationId == GameManager.Instance.myEntityPlayerLocal.entityId)
        {
            ModLogger.DebugLog("Skipping local player starting server");
            return false;
        }
        return true;
#else
        if (destinationId == -1)
        {
            return false;
        }

        return GameManager.IsDedicatedServer ||
               destinationId != GameManager.Instance.myEntityPlayerLocal.entityId;
#endif
    }

    public static void RefreshSingleLocks()
    {
        const string d_MethodName = nameof(RefreshSingleLocks);

        var lockManager = LockManager.Instance;
        if (lockManager == null)
        {
            ModLogger.DebugLog($"{d_MethodName} LockManager is null or otherwise could not obtain an instance of it");
            return;
        }

        var lockedDict = new Dictionary<Vector3i, int>();

        foreach (var (playerId, lockEntries) in lockManager.singleLocks)
        {
            foreach (var entry in lockEntries)
            {
                if (TryGetTileEntityPosition(entry.Target, out Vector3i pos))
                    lockedDict[pos] = playerId;
            }
        }

        BroadcastLockedEntitiesUpdate(lockedDict);
    }

    private static bool TryGetTileEntityPosition(ILockTarget target, out Vector3i position)
    {
        position = default;

        TileEntity tileEntity;
        if (target is TileEntity te)
        {
            tileEntity = te;
        }
        else if (target is TEFeatureAbs feature && feature.Parent != null)
        {
            tileEntity = feature.Parent;
        }
        else
        {
            return false;
        }

        if (tileEntity.TryGetSelfOrFeature(out ITileEntityLootable lootable))
        {
            if (!lootable.bPlayerStorage)
            {
                return false;
            }
            position = lootable.ToWorldPos();
            return true;
        }

        switch (tileEntity)
        {
            case TileEntityCollector collector:
                position = collector.ToWorldPos();
                return true;
            case TileEntityWorkstation workstation:
                position = workstation.ToWorldPos();
                return true;
            default:
                return false;
        }
    }

    private static void BroadcastLockedEntitiesUpdate(Dictionary<Vector3i, int> lockedDict)
    {
        SingletonMonoBehaviour<ConnectionManager>.Instance?.SendPackage(new NetPackageLockedTEs().Setup(lockedDict));

        TileEntityLocks.UpdateLockedTEs(lockedDict);
    }
}