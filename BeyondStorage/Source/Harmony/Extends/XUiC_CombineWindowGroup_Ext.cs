using BeyondStorage.Game.UI;
using BeyondStorage.Infrastructure;
using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

[HarmonyPatch(typeof(XUiC_CombineWindowGroup))]
internal static class XUiC_CombineWindowGroup_Ext
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_CombineWindowGroup.OnOpen))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_CombineWindowGroup_OnOpen_Postfix(XUiC_CombineWindowGroup __instance)
    {
        const string d_MethodName = nameof(XUiC_CombineWindowGroup_OnOpen_Postfix);

        // Check for duplicate window open (should not happen)
        if (WindowStateManager.IsCombineWindowOpen())
        {
            ModLogger.Error($"{d_MethodName}: Combine Window is already open. This should not happen!");
        }

        WindowStateManager.OnCombineWindowOpening(__instance);

        WindowStateManager.RefreshUseablesWindowBindings();

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Combine Window Opened");
#endif
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_CombineWindowGroup.OnClose))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_CombineWindowGroup_OnClose_Postfix(XUiC_CombineWindowGroup __instance)
    {
#if DEBUG
        //const string d_MethodName = nameof(XUiC_CombineWindowGroup_OnClose_Postfix);
#endif

        WindowStateManager.OnCombineWindowClosing(__instance);

        WindowStateManager.RefreshUseablesWindowBindings();

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Combine Window Closed");
#endif
    }
}
