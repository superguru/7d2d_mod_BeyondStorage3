using System.Collections.Concurrent;
using System.Collections.Generic;
using BeyondStorage.Infrastructure;
using BeyondStorage.Multiplayer;
using BeyondStorage.Source.Game.Files;
using BeyondStorage.Storage;
using BeyondStorage.UI;

namespace BeyondStorage.Entities;

internal static class BlockConsumeStates
{
    private static ConcurrentDictionary<Vector3i, byte> DisabledConsumptionBlocks { get; } = new();

    private static readonly MethodCallTracker s_methodStats = new("BlockConsumeStates");

    public static void Init()
    {
        s_methodStats.Clear();
        BlockConsumeStatePersistence.LoadDisabledBlocks(DisabledConsumptionBlocks);
    }

    public static void Cleanup()
    {
        DisabledConsumptionBlocks.Clear();
        s_methodStats.Clear();
    }

    internal static bool IsConsumeOn(Vector3i block) => !IsConsumeOff(block);

    internal static bool IsConsumeOff(Vector3i block) => DisabledConsumptionBlocks.ContainsKey(block);

    public static void TurnConsumeOff(Vector3i block)
    {
#if DEBUG
        const string d_MethodName = nameof(TurnConsumeOff);
#endif
        if (!DisabledConsumptionBlocks.TryAdd(block, 0))
        {
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: Block {block} already has consume turned off");
#endif
            return;
        }

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Block {block} consume turned off");
#endif

        if (IsMultiplayerClient())
        {
            SendChangeToServer(block, isConsumeOff: true);
            InvalidateLocalState();
            return;
        }

        OnBlockConsumeStateChanged();
    }

    public static void TurnConsumeOn(Vector3i block)
    {
#if DEBUG
        const string d_MethodName = nameof(TurnConsumeOn);
#endif
        if (!DisabledConsumptionBlocks.TryRemove(block, out _))
        {
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: Block {block} consume is already on");
#endif
            return;
        }

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Block {block} consume turned on");
#endif

        if (IsMultiplayerClient())
        {
            SendChangeToServer(block, isConsumeOff: false);
            InvalidateLocalState();
            return;
        }

        OnBlockConsumeStateChanged();
    }

    // Called on client when server sends the full authoritative set (on join or after any change)
    internal static void ApplyFromServer(List<Vector3i> disabledBlocks)
    {
        DisabledConsumptionBlocks.Clear();
        foreach (var pos in disabledBlocks)
        {
            DisabledConsumptionBlocks.TryAdd(pos, 0);
        }
        InvalidateLocalState();
    }

    // Called on server when it receives a change request from a client
    internal static void ApplyServerSideChange(Vector3i position, bool isConsumeOff)
    {
#if DEBUG
        const string d_MethodName = nameof(ApplyServerSideChange);
        ModLogger.DebugLog($"{d_MethodName}: pos {position}, isConsumeOff {isConsumeOff}");
#endif
        if (isConsumeOff)
        {
            DisabledConsumptionBlocks.TryAdd(position, 0);
        }
        else
        {
            DisabledConsumptionBlocks.TryRemove(position, out _);
        }
        OnBlockConsumeStateChanged();
    }

    // Called by ServerUtils to send the current state to a newly joined client
    internal static void SendConsumeStatesToClient(ClientInfo client)
    {
        var keys = new List<Vector3i>(DisabledConsumptionBlocks.Keys);
        client.SendPackage(NetPackageManager.GetPackage<NetPackageConsumeStates>().Setup(keys));
#if DEBUG
        ModLogger.DebugLog($"SendConsumeStatesToClient: {keys.Count} blocks to entity {client.entityId}");
#endif
    }

    private static void OnBlockConsumeStateChanged()
    {
        BlockConsumeStatePersistence.SaveDisabledBlocks(DisabledConsumptionBlocks);
        BroadcastConsumeStatesToClients();
        InvalidateLocalState();
    }

    private static void BroadcastConsumeStatesToClients()
    {
        var cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (cm == null || cm.IsSinglePlayer || !cm.IsServer)
        {
            return;
        }
        var keys = new List<Vector3i>(DisabledConsumptionBlocks.Keys);
        cm.SendPackage(NetPackageManager.GetPackage<NetPackageConsumeStates>().Setup(keys));
    }

    private static void InvalidateLocalState()
    {
        StorageContextFactory.InvalidateCache();
        UIRefreshHelper.RefreshAllWindows(nameof(OnBlockConsumeStateChanged), isStackOperation: false);
    }

    private static bool IsMultiplayerClient()
    {
        var cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
        return cm != null && !cm.IsSinglePlayer && !cm.IsServer;
    }

    private static void SendChangeToServer(Vector3i position, bool isConsumeOff)
    {
        var cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (cm == null)
        {
            return;
        }
        cm.SendToServer(NetPackageManager.GetPackage<NetPackageConsumeStateChange>().Setup(position, isConsumeOff));
    }
}
