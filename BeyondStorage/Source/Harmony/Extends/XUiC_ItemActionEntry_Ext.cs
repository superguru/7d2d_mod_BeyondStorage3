using System.Collections.Generic;
using Audio;
using BeyondStorage.Harmony.Components;
using BeyondStorage.Infrastructure;
using BeyondStorage.Storage;
using HarmonyLib;
using UnityEngine;

namespace BeyondStorage.Harmony.Extends;

/// <summary>
/// Makes the item info panel's action buttons behave correctly when the selected item is one of
/// the Useables grid's synthetic cells.
///
/// The panel is populated with <c>ItemActionListTypes.Item</c> (see
/// <c>XUiC_BeyondStorage_UseablesWindow.ShowItemInfo</c>) so it lists the same actions a real
/// backpack slot would. But those entries are built by vanilla for a cell that maps to an actual
/// inventory slot, and every one of them reaches straight past our storage layer to whatever
/// <c>ItemController</c> they're handed — which here is a display-only stack. Left alone that is a
/// duplication bug, not just a cosmetic mismatch:
///
///  - <c>ItemActionEntryUse</c> would apply the eat/drink/heal effect and decrement only the
///    cell's display count, never touching storage — free items, forever.
///  - <c>ItemActionEntryDrop</c> would call <c>ItemDropServer</c> with the cell's stack, i.e.
///    spawn the aggregated count of <i>real</i> items on the ground out of nothing.
///  - <c>ItemActionEntryScrap</c> would queue a scrap job and decrement the cell's display count
///    only; <c>ItemActionEntryCombine</c> would move the display stack into the combine grid.
///
/// <c>XUiC_ItemActionEntry.OnPressAction</c> is the single dispatch point for every action-list
/// button (mouse, keyboard and gamepad all funnel through it — there is no second call site of
/// <c>BaseItemActionEntry.OnActivated</c> in 3.3), so one prefix covers them all:
///  - Use  -> routed to <c>XUiC_BeyondStorage_UseablesGrid.TryUseSlot</c>, the same path as the
///    1-6 hotkeys and double-click, so the unit comes out of storage properly.
///  - Drop -> one unit is pulled out of storage and dropped in the world, so the world ends up
///    holding exactly what storage gave up.
///  - Recipes -> left to vanilla; it only opens the crafting list filtered by the item, and
///    consumes nothing.
///  - anything else -> denied with a tooltip rather than silently corrupting cell/storage state.
/// </summary>
[HarmonyPatch(typeof(XUiC_ItemActionEntry))]
internal static class XUiC_ItemActionEntry_Ext
{
    // Vanilla parameter name reused so Harmony binds by name; unused otherwise.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(XUiC_ItemActionEntry.OnPressAction))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static bool XUiC_ItemActionEntry_OnPressAction_Prefix(XUiC_ItemActionEntry __instance)
    {
        var entry = __instance.itemActionEntry;
        if (entry == null)
        {
            return true;
        }

        var cell = entry.ItemController as XUiC_ItemStack;
        if (cell == null)
        {
            return true;
        }

        var useablesGrid = cell.GetParentByType<XUiC_BeyondStorage_UseablesGrid>();
        if (useablesGrid == null)
        {
            // Not one of ours — a real backpack/toolbelt/loot cell. Vanilla all the way.
            return true;
        }

        // Recipes only filters the crafting list by this item: no consumption, no cell mutation.
        // Letting vanilla handle it keeps the button working normally.
        if (entry is ItemActionEntryRecipes)
        {
            return true;
        }

        if (!entry.Enabled)
        {
            // Mirror vanilla's "can't do that right now" feedback.
            Manager.PlayInsidePlayerHead(entry.DisabledSound);
            entry.OnDisabledActivate();
            return false;
        }

        Manager.PlayInsidePlayerHead(entry.SoundName);

        switch (entry)
        {
            case ItemActionEntryUse:
                // Same path as the 1-6 hotkeys / double-click, so TryUseSlot re-runs every
                // eligibility check and removes the unit from storage before applying the action.
                useablesGrid.TryUseSlot(cell.SlotNumber);
                break;

            case ItemActionEntryDrop:
                DropOneFromStorage(useablesGrid, cell);
                break;

            default:
                DenyUnsupportedAction(entry);
                break;
        }

        // Vanilla's post-activation button feedback, replayed here because we skip the original.
        __instance.background.Color = __instance.defaultBackgroundColor;
        __instance.background.SpriteName = "menu_empty";
        __instance.wasPressed = true;

        return false;
    }

    /// <summary>
    /// Withdraws a single unit of the cell's item from storage and drops it in the world, so the
    /// Drop button on a Useables cell is the storage-consistent equivalent of the one on a real
    /// inventory slot. The cell's stack is an aggregate across many sources and has no physical
    /// counterpart, so exactly one unit is taken rather than the displayed count.
    /// </summary>
    private static void DropOneFromStorage(XUiC_BeyondStorage_UseablesGrid useablesGrid, XUiC_ItemStack cell)
    {
        const string d_MethodName = nameof(DropOneFromStorage);

        var player = cell.xui.playerUI.entityPlayer;
        if (player == null || !ValidationHelper.ValidateStorageContext(d_MethodName, out var context))
        {
            return;
        }

        var itemValue = cell.ItemStack?.itemValue;
        if (itemValue == null || itemValue.IsEmpty())
        {
            return;
        }

        // Drop exactly what storage actually gave up (mod/meta included) rather than re-rolling
        // the display stack's count.
        var removed = new List<ItemStack>();
        if (context.RemoveRemaining(itemValue, 1, false, removed) != 1 || removed.Count == 0)
        {
            // Ranking was stale — nothing was removed, so just resync the display.
            useablesGrid.RefreshGridItems();
            return;
        }

        foreach (var stack in removed)
        {
            GameManager.Instance.ItemDropServer(stack, player.GetDropPosition(), Vector3.zero, player.entityId);
        }

        player.PlayOneShot("itemdropped");
        StorageContextFactory.InvalidateContext();
        useablesGrid.RefreshGridItems();
    }

    private static void DenyUnsupportedAction(BaseItemActionEntry entry)
    {
        var player = entry.ItemController?.xui?.playerUI?.entityPlayer;
        if (player != null)
        {
            GameManager.ShowTooltip(player, Localization.Get("xuiBeyondUseablesActionUnavailable"));
        }
    }
}