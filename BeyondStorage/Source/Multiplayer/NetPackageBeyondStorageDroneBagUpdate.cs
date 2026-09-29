using System;
using BeyondStorage.Infrastructure;
using UnityEngine.Scripting;

namespace BeyondStorage.Multiplayer;

/// <summary>
/// Carries a complete drone bag snapshot across the wire so peers can replace their
/// local view after the source player mutates the bag with smart push/pull. Mirrors
/// the fix applied to <see cref="NetPackageBeyondStorageVehicleBagUpdate"/> — the
/// native <c>Entity.OnBagModified</c> → <c>NetPackageBag</c> path is client→server
/// only and never rebroadcasts, leaving every other client with a stale view that
/// they would later stamp back over the authoritative state the next time they
/// interacted with the drone.
///
/// Drones use <see cref="XUiC_ItemStack.StackLocationTypes.Vehicle"/> for their
/// bag (same as vehicles), so the wire format is identical.
///
/// Sender writes use <c>StreamModeWrite.ToServer</c> on a client and
/// <c>StreamModeWrite.ToClient</c> on the server; receivers read with the matching
/// opposite direction.
/// </summary>
[Preserve]
public class NetPackageBeyondStorageDroneBagUpdate : NetPackage
{
    [Preserve] public int senderId;
    [Preserve] public int droneId;
    [Preserve] private readonly PooledExpandableMemoryStream bagData = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);

    /// <summary>
    /// Diagnostic-only: current size of the queued bag payload in bytes.
    /// </summary>
    public long BagDataSizeBytes => bagData.Length;

    public NetPackageBeyondStorageDroneBagUpdate Setup(EntityDrone _drone, int _senderId)
    {
        senderId = _senderId;
        droneId = _drone.entityId;
        bagData.SetLength(0L);
        bagData.Position = 0L;

        using PooledBinaryWriter bw = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
        bw.SetBaseStream(bagData);

        bool isServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
        _drone.bag.Write(bw, isServer ? StreamModeWrite.ToClient : StreamModeWrite.ToServer);

        return this;
    }

    [Preserve]
    ~NetPackageBeyondStorageDroneBagUpdate()
    {
        MemoryPools.poolMemoryStream.FreeSync(bagData);
    }

    public override void read(PooledBinaryReader _br)
    {
        senderId = _br.ReadInt32();
        droneId = _br.ReadInt32();
        int length = _br.ReadUInt16();
        StreamUtils.StreamCopy(_br.BaseStream, bagData, length);
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(senderId);
        _bw.Write(droneId);
        _bw.Write((ushort)bagData.Length);
        bagData.WriteTo(_bw.BaseStream);
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null)
        {
            return;
        }

        EntityDrone drone = _world.GetEntity(droneId) as EntityDrone;
        if (drone == null || drone.bag == null)
        {
            // Drone not active on this peer — same edge case as the native sync.
#if DEBUG
            ModLogger.DebugLog($"NetPackageBeyondStorageDroneBagUpdate: Dropped update — drone {droneId} not active (senderId {senderId}, payload {bagData.Length}B)");
#endif
            return;
        }

        bool isServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;

#if DEBUG
        ModLogger.DebugLog($"NetPackageBeyondStorageDroneBagUpdate: {(isServer ? "Server" : "Client")} received bag update for drone {droneId} from senderId {senderId} ({bagData.Length}B)");
#endif

        // Replace the local bag with the snapshot the sender just committed.
        // Drones use StackLocationTypes.Vehicle for their bags, same as vehicles.
        using PooledBinaryReader br = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
        lock (bagData)
        {
            br.SetBaseStream(bagData);
            bagData.Position = 0L;
            drone.bag = Bag.Read(br, XUiC_ItemStack.StackLocationTypes.Vehicle,
                isServer ? StreamModeRead.FromClient : StreamModeRead.FromServer,
                drone);
        }

#if DEBUG
        ModLogger.DebugLog($"NetPackageBeyondStorageDroneBagUpdate: Applied bag update for drone {droneId} on {(isServer ? "server" : "client")}");
#endif

        // Server-side rebroadcast to every client except the original sender.
        // A fresh package from the pool is used so the in-flight package (whose
        // bagData stream position is now at EOF) isn't recycled into another queue.
        if (isServer)
        {
            var rebroadcast = NetPackageManager.GetPackage<NetPackageBeyondStorageDroneBagUpdate>()
                                            .Setup(drone, senderId);
            var cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
            int recipientCount = (cm.Clients?.List.Count ?? 0) - (senderId == -1 ? 0 : 1);
            cm.SendPackage(rebroadcast, _onlyClientsAttachedToAnEntity: false, -1, senderId);
#if DEBUG
            ModLogger.DebugLog($"NetPackageBeyondStorageDroneBagUpdate: Rebroadcasting drone {droneId} bag ({rebroadcast.BagDataSizeBytes}B) from server to ~{recipientCount} client(s) (excluding senderId {senderId})");
#endif
        }
    }
}