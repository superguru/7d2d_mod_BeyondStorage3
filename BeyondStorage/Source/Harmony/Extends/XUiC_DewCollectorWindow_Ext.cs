using BeyondStorage.Game.UI;
using BeyondStorage.Infrastructure;
using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

[HarmonyPatch(typeof(XUiC_DewCollectorWindow))]
#if DEBUG
[HarmonyDebug]
#endif
internal static class XUiC_DewCollectorWindow_Ext
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_DewCollectorWindow.Init))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_DewCollectorWindow_Init_Postfix(XUiC_DewCollectorWindow __instance)
    {
#if DEBUG
        //const string d_MethodName = nameof(XUiC_DewCollectorWindow_Init_Postfix);
#endif
        var btnBeyondSmartButton = UIControlHelpers.GetSmartCollectorPushButton(__instance);
        if (btnBeyondSmartButton != null)
        {
            btnBeyondSmartButton.OnPress += SmartSortingCommon.SmartPushFromCollector_EventHandler;
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: Smart collector push button initialized");
#endif
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_DewCollectorWindow.OnOpen))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_DewCollectorWindow_OnOpen_Postfix(XUiC_DewCollectorWindow __instance)
    {
        const string d_MethodName = nameof(XUiC_DewCollectorWindow_OnOpen_Postfix);

        // Check for duplicate window open (should not happen)
        if (WindowStateManager.IsCollectorWindowOpen())
        {
            ModLogger.Error($"{d_MethodName}: Collector Window is already open. This should not happen!");
        }

        WindowStateManager.RefreshUseablesWindowBindings();

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Collector Window Opened");
#endif
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_DewCollectorWindow.OnClose))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_DewCollectorWindow_OnClose_Postfix(XUiC_DewCollectorWindow __instance)
    {
#if DEBUG
        //const string d_MethodName = nameof(XUiC_DewCollectorWindow_OnClose_Postfix);
#endif

        WindowStateManager.RefreshUseablesWindowBindings();

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Collector Window Closed");
#endif
    }
}