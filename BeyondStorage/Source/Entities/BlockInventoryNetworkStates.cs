using System.Collections.Concurrent;
using System.Collections.Generic;
using BeyondStorage.Game.Files;
using BeyondStorage.Infrastructure;
using BeyondStorage.Multiplayer;
using BeyondStorage.Storage;
using BeyondStorage.UI;

namespace BeyondStorage.Entities;

internal static class BlockInventoryNetworkStates
{
    private static ConcurrentDictionary<Vector3i, byte> DisabledBlocks { get; } = new();

    private static readonly MethodCallTracker s_methodStats = new("BlockInventoryNetworkStates");

    public static void Init()
    {
        s_methodStats.Clear();
        BlockInventoryNetworkStatePersistence.LoadDisabledBlocks(DisabledBlocks);
    }

    public static void Cleanup()
    {
        DisabledBlocks.Clear();
        s_methodStats.Clear();
    }

    internal static bool IsEnabledForInventoryNetwork(Vector3i block) => !IsDisabledForInventoryNetwork(block);

    internal static bool IsDisabledForInventoryNetwork(Vector3i block) => DisabledBlocks.ContainsKey(block);

    public static void TurnOffForInventoryNetwork(Vector3i block)
    {
#if DEBUG
        const string d_MethodName = nameof(TurnOffForInventoryNetwork);
#endif
        if (!DisabledBlocks.TryAdd(block, 0))
        {
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: Block {block} already turned off for inventory network");
#endif
            return;
        }

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Block {block} inventory network turned off");
#endif

        if (IsMultiplayerClient())
        {
            SendChangeToServer(block, isDisabledForInventoryNetwork: true);
            InvalidateLocalState();
            return;
        }

        OnBlockInventoryNetworkStateChanged();
    }

    public static void TurnOnForInventoryNetwork(Vector3i block)
    {
#if DEBUG
        const string d_MethodName = nameof(TurnOnForInventoryNetwork);
#endif
        if (!DisabledBlocks.TryRemove(block, out _))
        {
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: Block {block} turned on for inventory network");
#endif
            return;
        }

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Block {block} inventory network turned on");
#endif

        if (IsMultiplayerClient())
        {
            SendChangeToServer(block, isDisabledForInventoryNetwork: false);
            InvalidateLocalState();
            return;
        }

        OnBlockInventoryNetworkStateChanged();
    }

    // Called on client when server sends the full authoritative set (on join or after any change)
    internal static void ApplyFromServer(List<Vector3i> disabledBlocks)
    {
        DisabledBlocks.Clear();
        foreach (var pos in disabledBlocks)
        {
            DisabledBlocks.TryAdd(pos, 0);
        }
        InvalidateLocalState();
    }

    // Called on server when it receives a change request from a client
    internal static void ApplyServerSideChange(Vector3i position, bool isDisabledForInventoryNetwork)
    {
#if DEBUG
        const string d_MethodName = nameof(ApplyServerSideChange);
        ModLogger.DebugLog($"{d_MethodName}: pos {position}, isDisabledForInventoryNetwork {isDisabledForInventoryNetwork}");
#endif
        if (isDisabledForInventoryNetwork)
        {
            DisabledBlocks.TryAdd(position, 0);
        }
        else
        {
            DisabledBlocks.TryRemove(position, out _);
        }
        OnBlockInventoryNetworkStateChanged();
    }

    // Called by ServerUtils to send the current state to a newly joined client
    internal static void SendBlockInventoryNetworkStatesToClient(ClientInfo client)
    {
#if DEBUG
        const string d_MethodName = nameof(SendBlockInventoryNetworkStatesToClient);
#endif
        var keys = new List<Vector3i>(DisabledBlocks.Keys);
        client.SendPackage(NetPackageManager.GetPackage<NetPackageInventoryNetworkStates>().Setup(keys));
#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: {keys.Count} blocks to entity {client.entityId}");
#endif
    }

    private static void OnBlockInventoryNetworkStateChanged()
    {
        BlockInventoryNetworkStatePersistence.SaveDisabledBlocks(DisabledBlocks);
        BroadcastInventoryNetworkStatesToClients();
        InvalidateLocalState();
    }

    private static void BroadcastInventoryNetworkStatesToClients()
    {
        var cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (cm == null || cm.IsSinglePlayer || !cm.IsServer)
        {
            return;
        }
        var keys = new List<Vector3i>(DisabledBlocks.Keys);
        cm.SendPackage(NetPackageManager.GetPackage<NetPackageInventoryNetworkStates>().Setup(keys));
    }

    private static void InvalidateLocalState()
    {
        StorageContextFactory.InvalidateCache();
        UIRefreshHelper.RefreshAllWindows(nameof(OnBlockInventoryNetworkStateChanged), isStackOperation: false);
    }

    private static bool IsMultiplayerClient()
    {
        var cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
        return cm != null && !cm.IsSinglePlayer && !cm.IsServer;
    }

    private static void SendChangeToServer(Vector3i position, bool isDisabledForInventoryNetwork)
    {
        var cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (cm == null)
        {
            return;
        }
        cm.SendToServer(NetPackageManager.GetPackage<NetPackageInventoryNetworkStateChange>().Setup(position, isDisabledForInventoryNetwork));
    }
}
