using BeyondStorage.Game.UI;
using HarmonyLib;

namespace BeyondStorage.Source.Harmony.Extends;

///
/// Introduced in v3.0.0 of 7 Days to Die, where it was only used for vehicles
/// 

[HarmonyPatch(typeof(XUiC_BagStorageWindowGroup))]
internal static class XUiC_BagStorageWindowGroup_Ext
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_BagStorageWindowGroup.OnOpen))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_BagStorageWindowGroup_OnOpen_Postfix(XUiC_BagStorageWindowGroup __instance)
    {
#if DEBUG
        //const string d_MethodName = nameof(XUiC_BagStorageWindowGroup_OnOpen_Postfix);
#endif

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName} Start");
#endif

        WindowStateManager.OnBagStorageWindowOpening(__instance);

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: End. Bag Storage Window Opened for {__instance.lootContainer?.Name}");
#endif
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_BagStorageWindowGroup.OnClose))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_BagStorageWindowGroup_OnClose_Postfix(XUiC_BagStorageWindowGroup __instance)
    {
#if DEBUG
        //const string d_MethodName = nameof(XUiC_BagStorageWindowGroup_OnClose_Postfix);
#endif

        WindowStateManager.OnBagStorageWindowClosing(__instance);

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Bag Storage Window Closed");
#endif
    }
}
