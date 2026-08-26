using BeyondStorage.Game.UI;
using BeyondStorage.Infrastructure;
using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

[HarmonyPatch(typeof(XUiC_CharacterFrameWindow))]
internal static class XUiC_CharacterFrameWindow_Ext
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_CharacterFrameWindow.OnOpen))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_CharacterFrameWindow_OnOpen_Postfix(XUiC_CharacterFrameWindow __instance)
    {
        const string d_MethodName = nameof(XUiC_CharacterFrameWindow_OnOpen_Postfix);

        // Check for duplicate window open (should not happen)
        if (WindowStateManager.IsCharacterFrameWindowOpen())
        {
            ModLogger.Error($"{d_MethodName}: Character Frame Window is already open. This should not happen!");
        }

        WindowStateManager.OnCharacterFrameWindowOpening(__instance);

        WindowStateManager.RefreshUseablesWindowBindings();

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Collector Window Group Opened");
#endif
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_CharacterFrameWindow.OnClose))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_CharacterFrameWindow_OnClose_Postfix(XUiC_CharacterFrameWindow __instance)
    {
#if DEBUG
        //const string d_MethodName = nameof(XUiC_CharacterFrameWindow_OnClose_Postfix);
#endif

        WindowStateManager.OnCharacterFrameWindowClosing(__instance);

        WindowStateManager.RefreshUseablesWindowBindings();

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Collector Window Closed");
#endif
    }
}