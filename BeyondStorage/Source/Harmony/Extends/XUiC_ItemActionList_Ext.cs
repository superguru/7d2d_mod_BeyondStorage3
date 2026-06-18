using BeyondStorage.Game.Item;
using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

[HarmonyPatch(typeof(XUiC_ItemActionList))]
internal static class XUiC_ItemActionList_Ext
{
    // Used For:
    //      Item Repair (tracks item action list visibility)
    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_ItemActionList.Init))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_ItemActionList_Init_Postfix(XUiC_ItemActionList __instance)
    {
        __instance.OnVisiblity += ActionList_VisibilityChanged;
    }

    // Capture when the visibility of the Action List is changed
    private static void ActionList_VisibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
    {
        ItemRepair.ActionListVisible = _visibleSelf || _visibleInScene;
    }
}