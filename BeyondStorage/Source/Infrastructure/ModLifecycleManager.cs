using BeyondStorage.Data;
using BeyondStorage.Entities;
using BeyondStorage.Multiplayer;
using BeyondStorage.UI;
using static ModEvents;

namespace BeyondStorage.Infrastructure;

public static class ModLifecycleManager
{
    public static void GameStartDone(ref SGameStartDoneData data)
    {
        ModLogger.DebugLog("Game Start: Initializing...");

        TileEntityLocks.Init();

        BuildConsumeCapabilityCheckList();
        BlockConsumeStates.Init();

        InitSinglePlayer();
    }

    private static void BuildConsumeCapabilityCheckList()
    {
        ConsumeCapabilityCheckList.AddCheck(ConsumeCapabilityChecker_TEFeatureStorage.CanToggleConsume);
    }

    private static void InitSinglePlayer()
    {
        if (!WorldTools.IsSinglePlayer())
        {
            return;
        }

        // The purpose of this is to avoid a flicker on the currency display, visible when first purchasing from a Trader due to cache initialisation
        var itemStack = CurrencyCache.GetEmptyCurrencyStack();
        UIRefreshHelper.LogAndRefreshUI(StackOps.Stack_LockStateChange_Operation, itemStack: itemStack);
    }

    public static void GameShutdown(ref SGameShutdownData data)
    {
        ModLogger.DebugLog("Game Shutdown: Cleaning up...");
        TileEntityLocks.Cleanup();
        BlockConsumeStates.Cleanup();
    }
}