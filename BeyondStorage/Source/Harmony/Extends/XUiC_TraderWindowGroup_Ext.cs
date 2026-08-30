using BeyondStorage.Game.UI;
using BeyondStorage.Infrastructure;
using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

[HarmonyPatch(typeof(XUiC_TraderWindowGroup))]
internal static class XUiC_TraderWindowGroup_Ext
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_TraderWindowGroup.OnOpen))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_TraderWindowGroup_OnOpen_Postfix(XUiC_TraderWindowGroup __instance)
    {
        const string d_MethodName = nameof(XUiC_TraderWindowGroup_OnOpen_Postfix);

        // Check for duplicate window open (should not happen)
        if (WindowStateManager.IsTraderWindowOpen())
        {
            ModLogger.Error($"{d_MethodName}: Trader Window is already open. This should not happen!");
        }

        WindowStateManager.OnTraderWindowOpening(__instance);

        WindowStateManager.RefreshUseablesWindowBindings();

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Trader Window Opened");
#endif
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_TraderWindowGroup.OnClose))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_TraderWindowGroup_OnClose_Postfix(XUiC_TraderWindowGroup __instance)
    {
#if DEBUG
        //const string d_MethodName = nameof(XUiC_TraderWindowGroup_OnClose_Postfix);
#endif

        WindowStateManager.OnTraderWindowClosing(__instance);

        WindowStateManager.RefreshUseablesWindowBindings();

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Trader Window Closed");
#endif
    }
}
