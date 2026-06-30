using System.Collections.Concurrent;
using BeyondStorage.Infrastructure;
using BeyondStorage.Storage;
using BeyondStorage.UI;

namespace BeyondStorage.Entities;

internal static class BlockConsumeStates
{
    private static ConcurrentDictionary<Vector3i, bool> DisabledConsumptionBlocks
    {
        get; set;
    }

    private static readonly MethodCallTracker s_methodStats = new("BlockConsumeStates");

    public static void Init()
    {
        DisabledConsumptionBlocks = new ConcurrentDictionary<Vector3i, bool>();
        s_methodStats.Clear();

        //TODO: Load previous list from game save in single player
        //TODO: Request sync from server in multiplayer
    }

    public static void Cleanup()
    {
        DisabledConsumptionBlocks?.Clear();
        s_methodStats.Clear();
    }

    internal static bool IsConsumeOn(Vector3i block)
    {
        return !IsConsumeOff(block);
    }

    internal static bool IsConsumeOff(Vector3i block)
    {
        return DisabledConsumptionBlocks.ContainsKey(block);
    }

    public static void TurnConsumeOff(Vector3i block)
    {
        const string d_MethodName = nameof(TurnConsumeOff);

        if (!DisabledConsumptionBlocks.TryAdd(block, true))
        {
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: Block {block} already has consume turned off");
#endif
            return;
        }

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Block {block} consume turned off");
#endif

        OnBlockConsumeStateChanged();

        // TODO: Send NetPackage to sync in multiplayer, otherwise Save in singleplayer. Or always Save?
    }

    public static void TurnConsumeOn(Vector3i block)
    {
        const string d_MethodName = nameof(TurnConsumeOn);

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

        OnBlockConsumeStateChanged();

        // TODO: Send NetPackage to sync in multiplayer, otherwise Save in singleplayer. Or always Save?
    }

    private static void OnBlockConsumeStateChanged()
    {
        StorageContextFactory.InvalidateCache();
        UIRefreshHelper.RefreshAllWindows(nameof(OnBlockConsumeStateChanged), isStackOperation: false);
    }
}
