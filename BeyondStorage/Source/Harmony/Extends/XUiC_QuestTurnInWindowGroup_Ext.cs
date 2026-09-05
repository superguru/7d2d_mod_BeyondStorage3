using BeyondStorage.Game.UI;
using BeyondStorage.Infrastructure;
using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

[HarmonyPatch(typeof(XUiC_QuestTurnInWindowGroup))]
internal static class XUiC_QuestTurnInWindowGroup_Ext
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_QuestTurnInWindowGroup.OnOpen))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_QuestTurnInWindowGroup_OnOpen_Postfix(XUiC_QuestTurnInWindowGroup __instance)
    {
        const string d_MethodName = nameof(XUiC_QuestTurnInWindowGroup_OnOpen_Postfix);

        // Check for duplicate window open (should not happen)
        if (WindowStateManager.IsQuestTurnInWindowOpen())
        {
            ModLogger.Error($"{d_MethodName}: Quest Turn-In Window is already open. This should not happen!");
        }

        WindowStateManager.OnQuestTurnInWindowOpening(__instance);

        WindowStateManager.RefreshUseablesWindowBindings();

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Quest Turn-In Window Opened");
#endif
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_QuestTurnInWindowGroup.OnClose))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_QuestTurnInWindowGroup_OnClose_Postfix(XUiC_QuestTurnInWindowGroup __instance)
    {
#if DEBUG
        //const string d_MethodName = nameof(XUiC_QuestTurnInWindowGroup_OnClose_Postfix);
#endif

        WindowStateManager.OnQuestTurnInWindowClosing(__instance);

        WindowStateManager.RefreshUseablesWindowBindings();

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Quest Turn-In Window Closed");
#endif
    }
}
