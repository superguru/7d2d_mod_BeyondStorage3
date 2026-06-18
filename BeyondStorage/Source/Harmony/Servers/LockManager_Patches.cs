using System.Collections.Generic;
using System.Reflection;
using BeyondStorage.Multiplayer;
using HarmonyLib;

#if DEBUG
using BeyondStorage.Infrastructure;
#endif

namespace BeyondStorage.Harmony.Servers;

[HarmonyPatch]
internal static class LockManager_Patches
{
#if DEBUG
    [HarmonyPrepare]
    private static void Prepare(MethodBase original)
    {
        if (original == null)
        {
            return;
        }

        ModLogger.DebugLog($"Adding Postfix to {typeof(LockManager)}.{original.Name} | {original}");
    }
#endif

    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        // Lock is Granted
        yield return AccessTools.Method(typeof(LockManager), nameof(LockManager.LockRequestServer));                      // public void ChangeBlocks(PlatformUserIdentifierAbs persistentPlayerId, List<BlockChangeInfo> _blocksToChange)

        // Lock is Released
        yield return AccessTools.Method(typeof(LockManager), nameof(LockManager.UnlockRequestServer));                      // public void ChangeBlocks(PlatformUserIdentifierAbs persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
        yield return AccessTools.Method(typeof(LockManager), nameof(LockManager.ForceUnlockByPlayer));                      // public void ChangeBlocks(PlatformUserIdentifierAbs persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
        yield return AccessTools.Method(typeof(LockManager), nameof(LockManager.ForceUnlockByChunk));                      // public void ChangeBlocks(PlatformUserIdentifierAbs persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        // Skip if we're not a server
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            return;
        }

        // Skip if single player
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsSinglePlayer)
        {
            return;
        }

        // Otherwise update the single locks list. Shared locks are for traders.
        ServerUtils.RefreshSingleLocks();
    }
}