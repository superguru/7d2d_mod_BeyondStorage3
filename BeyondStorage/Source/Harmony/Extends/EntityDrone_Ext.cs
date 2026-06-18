using BeyondStorage.Game.Item;
using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

[HarmonyPatch(typeof(EntityDrone))]
internal static class EntityDrone_Ext
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(EntityDrone.DoRepairAction))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static bool EntityDrone_DoRepairAction_Prefix(EntityDrone __instance, LocalPlayerUI playerUI)
    {
        const string repairKitName = "resourceRepairKit";

        // Bag/inventory has the kit — let the original handle it unchanged
        if (__instance.HasStoredItem(playerUI.entityPlayer, repairKitName, EntityDrone.repairKitTags))
        {
            return true;
        }

        // Drone doesn't need repair — let original run (no-ops cleanly)
        if (__instance.GetRepairAmountNeeded() <= 0)
        {
            return true;
        }

        ItemValue repairKitItem = ItemClass.GetItem(repairKitName);

        // Nothing in storage either — let original play the "missing item" sound
        if (!ItemCommon.HasItemInStorage(repairKitItem))
        {
            return true;
        }

        // Storage has a repair kit — consume it and repair, mirroring original call order
        playerUI.xui.CollectedItemList.RemoveItemStack(new ItemStack(repairKitItem, 1));
        __instance.PlaySound("crafting/craft_repair_item");
        ItemCommon.ItemRemoveRemaining(repairKitItem, 1);
        __instance.performRepair();
        __instance.SendSyncData(16);

        return false; // Skip original
    }
}
