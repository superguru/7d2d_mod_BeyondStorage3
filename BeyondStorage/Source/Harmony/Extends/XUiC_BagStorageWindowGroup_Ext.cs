using BeyondStorage.Game.UI;
using BeyondStorage.Infrastructure;
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
        const string d_MethodName = nameof(XUiC_BagStorageWindowGroup_OnOpen_Postfix);

        // Check for duplicate window open (should not happen)
        if (WindowStateManager.IsBagStorageWindowOpen())
        {
            ModLogger.Error($"{d_MethodName}: Bag Storage Window is already open. This should not happen!");
        }

        WindowStateManager.OnBagStorageWindowOpened(__instance);

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Bag Storage Window Opened");
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

        WindowStateManager.OnBagStorageWindowClosed(__instance);

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Bag Storage Window Closed");
#endif
    }
}
