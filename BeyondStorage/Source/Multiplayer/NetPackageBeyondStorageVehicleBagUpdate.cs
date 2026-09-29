using System;
using BeyondStorage.Infrastructure;
using UnityEngine.Scripting;

namespace BeyondStorage.Multiplayer;

/// <summary>
/// Carries a complete vehicle bag snapshot across the wire so peers (listen-server
/// clients or dedicated-server clients) can replace their local view after the source
/// player mutates the bag with smart push/pull. The native <c>NetPackageVehicleDataSync</c>
/// path doesn't suffice for this because <c>GetSyncFlagsReplicated</c> strips the bag
/// flag, so a remote smart push was never propagated — leaving every other client
/// with a stale view that they would later stamp back over the authoritative state
/// the next time they interacted with the vehicle.
///
/// Sender writes use <c>StreamModeWrite.ToServer</c> on a client and
/// <c>StreamModeWrite.ToClient</c> on the server; receivers read with the matching
/// opposite direction.
/// </summary>
[Preserve]
public class NetPackageBeyondStorageVehicleBagUpdate : NetPackage
{
    [Preserve] public int senderId;
    [Preserve] public int vehicleId;
    [Preserve] private readonly PooledExpandableMemoryStream bagData = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);

    /// <summary>
    /// Diagnostic-only: current size of the queued bag payload in bytes, useful for log lines
    /// that confirm "yes, we did send/receive a payload" without having to dive into the stream.
    /// </summary>
    public long BagDataSizeBytes => bagData.Length;

    public NetPackageBeyondStorageVehicleBagUpdate Setup(EntityVehicle _vehicle, int _senderId)
    {
        senderId = _senderId;
        vehicleId = _vehicle.entityId;
        bagData.SetLength(0L);
        bagData.Position = 0L;

        using PooledBinaryWriter bw = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
        bw.SetBaseStream(bagData);

        bool isServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
        _vehicle.bag.Write(bw, isServer ? StreamModeWrite.ToClient : StreamModeWrite.ToServer);

        return this;
    }

    [Preserve]
    ~NetPackageBeyondStorageVehicleBagUpdate()
    {
        MemoryPools.poolMemoryStream.FreeSync(bagData);
    }

    public override void read(PooledBinaryReader _br)
    {
        senderId = _br.ReadInt32();
        vehicleId = _br.ReadInt32();
        int length = _br.ReadUInt16();
        StreamUtils.StreamCopy(_br.BaseStream, bagData, length);
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(senderId);
        _bw.Write(vehicleId);
        _bw.Write((ushort)bagData.Length);
        bagData.WriteTo(_bw.BaseStream);
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null)
        {
            return;
        }

        EntityVehicle vehicle = _world.GetEntity(vehicleId) as EntityVehicle;
        if (vehicle == null || vehicle.bag == null)
        {
            // Vehicle not active on this peer — same edge case as the native sync.
#if DEBUG
            ModLogger.DebugLog($"NetPackageBeyondStorageVehicleBagUpdate: Dropped update — vehicle {vehicleId} not active (senderId {senderId}, payload {bagData.Length}B)");
#endif
            return;
        }

        bool isServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;

#if DEBUG
        ModLogger.DebugLog($"NetPackageBeyondStorageVehicleBagUpdate: {(isServer ? "Server" : "Client")} received bag update for vehicle {vehicleId} from senderId {senderId} ({bagData.Length}B)");
#endif

        // Replace the local bag with the snapshot the sender just committed.
        // Same code path the native NetPackageVehicleDataSync uses for flag 8.
        using PooledBinaryReader br = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
        lock (bagData)
        {
            br.SetBaseStream(bagData);
            bagData.Position = 0L;
            vehicle.bag = Bag.Read(br, XUiC_ItemStack.StackLocationTypes.Vehicle,
                isServer ? StreamModeRead.FromClient : StreamModeRead.FromServer,
                vehicle);
        }

#if DEBUG
        ModLogger.DebugLog($"NetPackageBeyondStorageVehicleBagUpdate: Applied bag update for vehicle {vehicleId} on {(isServer ? "server" : "client")}");
#endif

        // Server-side rebroadcast to every client except the original sender.
        // A fresh package from the pool is used so the in-flight package (whose
        // bagData stream position is now at EOF) isn't recycled into another queue.
        if (isServer)
        {
            var rebroadcast = NetPackageManager.GetPackage<NetPackageBeyondStorageVehicleBagUpdate>()
                                            .Setup(vehicle, senderId);
            var cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
            int recipientCount = (cm.Clients?.List.Count ?? 0) - (senderId == -1 ? 0 : 1);
            cm.SendPackage(rebroadcast, _onlyClientsAttachedToAnEntity: false, -1, senderId);
#if DEBUG
            ModLogger.DebugLog($"NetPackageBeyondStorageVehicleBagUpdate: Rebroadcasting vehicle {vehicleId} bag ({rebroadcast.BagDataSizeBytes}B) from server to ~{recipientCount} client(s) (excluding senderId {senderId})");
#endif
        }
    }
}