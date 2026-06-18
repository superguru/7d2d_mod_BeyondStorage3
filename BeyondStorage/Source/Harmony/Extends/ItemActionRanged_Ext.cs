using BeyondStorage.Game.Item;
using BeyondStorage.Game.Ranged;
using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

[HarmonyPatch(typeof(ItemActionRanged))]
internal static class ItemActionRanged_Ext
{
    // Used For:
    //          Weapon Reload (check if allowed to reload)
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ItemActionRanged.CanReload))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static bool ItemActionRanged_CanReload_Prefix(ItemActionRanged __instance, ItemActionData _actionData, ref bool __result)
    {
        ItemActionRanged.ItemActionDataRanged actionData = (ItemActionRanged.ItemActionDataRanged)_actionData;
        ItemValue holdingItemItemValue = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
        ItemValue ammoItemValue = ItemClass.GetItem(__instance.MagazineItemNames[holdingItemItemValue.SelectedAmmoTypeIndex]);
        int magazineSize = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, holdingItemItemValue, __instance.BulletsPerMagazine, _actionData.invData.holdingEntity);
        EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;

        // Prerequisites: must not be currently reloading, not cancelling, and either jammed or magazine not full
        if (!ItemActionRanged.NotReloading(actionData) ||
            (entityPlayerLocal?.CancellingInventoryActions == true) ||
            (!__instance.isJammed(holdingItemItemValue) && _actionData.invData.itemValue.Meta >= magazineSize))
        {
            __result = false;
            return false;
        }

        // Ammo available in bag or toolbelt
        if (_actionData.invData.holdingEntity.bag.GetItemCount(ammoItemValue) > 0 ||
            _actionData.invData.holdingEntity.inventory.GetItemCount(ammoItemValue) > 0)
        {
            __result = true;
            return false;
        }

        // Fallback: infinite ammo or storage (mirrors original HasInfiniteAmmo fallback position)
        __result = __instance.HasInfiniteAmmo(_actionData) || ItemCommon.HasItemInStorage(ammoItemValue);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ItemActionRanged.CompleteReload))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static bool ItemActionRanged_CompleteReload_Prefix(ItemActionRanged __instance, ItemActionRanged.ItemActionDataRanged _adr)
    {
        if (_adr == null)
        {
            return false;
        }

        EntityAlive holdingEntity = _adr.invData.holdingEntity;
        ItemValue itemValue = ItemClass.GetItem(__instance.MagazineItemNames[_adr.invData.itemValue.SelectedAmmoTypeIndex]);
        int magazineSize = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, _adr.invData.itemValue, __instance.BulletsPerMagazine, holdingEntity);

        _adr.reloadAmount = AnimatorCommon.RemoveAndCountAmmoForReload(__instance, _adr, holdingEntity, itemValue, magazineSize);

        if (_adr.reloadAmount > 0)
        {
            _adr.invData.itemValue.Meta = Utils.FastMin(_adr.invData.itemValue.Meta + _adr.reloadAmount, magazineSize);
            if (_adr.invData.item.Properties.GetValue(ItemClass.PropSoundIdle) != null)
            {
                _adr.invData.holdingEntitySoundID = -1;
            }
        }

        _adr.invData.holdingEntity.inventory.holdingItemItemValue.RemoveMetaData(ItemActionRanged.scGunIsJammed);

        return false; // Skip original method
    }
}