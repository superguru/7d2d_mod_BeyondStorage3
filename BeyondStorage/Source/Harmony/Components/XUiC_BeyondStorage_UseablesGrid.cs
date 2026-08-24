using System.Collections.Generic;
using BeyondStorage.Data;
using BeyondStorage.Infrastructure;
using BeyondStorage.Storage;
using UnityEngine;
using UnityEngine.Scripting;

namespace BeyondStorage.Harmony.Components;

[Preserve]
public class XUiC_BeyondStorage_UseablesGrid : XUiC_BeyondStorage_ItemGrid
{
    // Matches windows.xml: <grid rows="2" cols="3" .../> — row 0 = Heal, row 1 = Food/Drink
    private const int ROW_SIZE = 3;
    private const int ROW_COUNT = 2;
    private const int TOTAL_SLOTS = ROW_SIZE * ROW_COUNT;

    // Row 1 target composition: 1 food + 2 drinks when both are available.
    private const int FOOD_QUOTA = 1;
    private const int DRINK_QUOTA = 2;

    public override void OnOpen()
    {
        base.OnOpen();
        RefreshTopItems();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    /// <summary>
    /// Repopulates the grid: row 0 (slots 1-3) with the top heal items ranked by heal amount then
    /// count; row 1 (slots 4-6) with 1 food + 2 drinks (falling back to whichever category has
    /// items if the other is empty), each ranked by nutrition value then net health effect. Cells
    /// are display-only synthetic stacks, not live references to a storage slot — see
    /// StorageSourceItemDataStore.GetTopItemsByScore for how the ranking avoids re-walking storage.
    /// </summary>
    internal void RefreshTopItems()
    {
        const string d_MethodName = nameof(RefreshTopItems);

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out var context))
        {
            SetStacks(BuildEmptySlots());
            return;
        }

        var healTop = context.GetTopUseableItemsByScore(UseableItemStore.IsHealItem, UseableItemStore.GetHealScore, ROW_SIZE);
        var foodTop = context.GetTopUseableItemsByScore(UseableItemStore.IsFoodItem, UseableItemStore.GetNutritionScore, ROW_SIZE);
        var drinkTop = context.GetTopUseableItemsByScore(UseableItemStore.IsDrinkItem, UseableItemStore.GetNutritionScore, ROW_SIZE);
        var foodDrinkRow = ComposeFoodDrinkRow(foodTop, drinkTop);

        var stacks = BuildEmptySlots();
        FillRow(stacks, rowStart: 0, topItems: healTop);
        FillRow(stacks, rowStart: ROW_SIZE, topItems: foodDrinkRow);

        SetStacks(stacks);
        LockCells();
    }

    /// <summary>
    /// Fills the food/drink row: up to <see cref="FOOD_QUOTA"/> food + <see cref="DRINK_QUOTA"/>
    /// drinks, then backfills any remaining slots from whichever list still has items (food first).
    /// This naturally degrades to "just food" when there are no drinks, or "just drinks" when there
    /// is no food, while otherwise preserving each list's own nutrition-based ranking.
    /// </summary>
    private static List<(int ItemType, int Count)> ComposeFoodDrinkRow(
        IReadOnlyList<(int ItemType, int Count)> foodTop,
        IReadOnlyList<(int ItemType, int Count)> drinkTop)
    {
        var result = new List<(int ItemType, int Count)>(ROW_SIZE);
        int foodIndex = 0;
        int drinkIndex = 0;

        for (int i = 0; i < FOOD_QUOTA && foodIndex < foodTop.Count; i++)
        {
            result.Add(foodTop[foodIndex++]);
        }

        for (int i = 0; i < DRINK_QUOTA && drinkIndex < drinkTop.Count; i++)
        {
            result.Add(drinkTop[drinkIndex++]);
        }

        while (result.Count < ROW_SIZE && (foodIndex < foodTop.Count || drinkIndex < drinkTop.Count))
        {
            if (foodIndex < foodTop.Count)
            {
                result.Add(foodTop[foodIndex++]);
            }
            else
            {
                result.Add(drinkTop[drinkIndex++]);
            }
        }

        return result;
    }

    private static ItemStack[] BuildEmptySlots()
    {
        var stacks = new ItemStack[TOTAL_SLOTS];
        for (int i = 0; i < stacks.Length; i++)
        {
            stacks[i] = ItemStack.Empty;
        }
        return stacks;
    }

    private static void FillRow(ItemStack[] stacks, int rowStart, IReadOnlyList<(int ItemType, int Count)> topItems)
    {
        for (int i = 0; i < topItems.Count && i < ROW_SIZE; i++)
        {
            var (itemType, count) = topItems[i];
            stacks[rowStart + i] = new ItemStack(new ItemValue(itemType), count);
        }
    }

    /// <summary>
    /// Uses whatever is currently displayed in <paramref name="slotIndex"/> (0-5), triggered by a
    /// number-key press or a double-click on the cell. Removes exactly 1 unit from storage first,
    /// then hands off to vanilla's own ItemActionEntryUse.OnActivated on this cell's controller so
    /// animation/prompt/buff/XP/jar-refund logic is identical to using the item normally — this
    /// mod only needs to source the 1 unit from storage instead of a real backpack/toolbelt slot.
    /// </summary>
    internal void TryUseSlot(int slotIndex)
    {
        const string d_MethodName = nameof(TryUseSlot);

        var controllers = GetItemStackControllers();
        if (slotIndex < 0 || slotIndex >= controllers.Length)
        {
            return;
        }

        var cellController = controllers[slotIndex];
        var itemStack = cellController.ItemStack;
        if (itemStack == null || itemStack.IsEmpty())
        {
            return;
        }

        var itemValue = itemStack.itemValue;
        var consumeType = GetConsumeTypeForSlot(slotIndex, itemValue.type);

        if (!CanUseNow(consumeType))
        {
            return;
        }

        // Skip prompted items (e.g. some medical items ask "use on self?") rather than risk
        // stranding an already-removed unit behind a dialog the player might cancel.
        if (RequiresPrompt(itemValue))
        {
            return;
        }

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out var context))
        {
            return;
        }

        var removedCount = context.RemoveRemaining(itemValue, 1);
        if (removedCount != 1)
        {
            // Ranking was stale (e.g. someone/something else consumed it since the last refresh) —
            // nothing was removed, so just resync the display instead of using a phantom item.
            RefreshTopItems();
            return;
        }

        cellController.ItemStack = new ItemStack(itemValue.Clone(), 1);

        // ItemActionEntryUse.OnActivated (and the coroutine it may schedule for animated eat/drink
        // actions) expects ParentActionList to be set — vanilla only ever constructs this via
        // XUiC_ItemActionList.AddActionListEntry, which we bypass entirely. Without it,
        // ItemActionEntryUse.SwitchBackCoroutine null-refs on base.ParentActionList.RefreshActionList()
        // AFTER the eat animation finishes (a separate Unity coroutine tick we can't try/catch around
        // from here), which aborts before it can reset xui.IsUsingItemActionEntryUse — permanently
        // soft-locking every future use (vanilla included) behind the "isBusy" check. A bare instance
        // is enough: RefreshActionList() only ever does `IsDirty = true`, and this one is never
        // attached to the UI tree or rendered.
        var entry = new ItemActionEntryUse(cellController, consumeType)
        {
            ParentActionList = new XUiC_ItemActionList()
        };

        try
        {
            entry.OnActivated();
        }
        catch (System.Exception ex)
        {
            // Covers only the synchronous portion of OnActivated — an exception from a coroutine it
            // schedules for animated actions happens on a later, unrelated Unity tick and can't be
            // caught here. MarkUsePending's watchdog (checked every frame) is the backstop for that.
            ModLogger.Error($"{d_MethodName}: OnActivated threw for slot {slotIndex} ({itemValue.ItemClass?.GetItemName()}). Forcing busy-state recovery.", ex);
            xui.IsUsingItemActionEntryUse = false;
            cellController.HiddenLock = false;
        }

        MarkUsePending();
        StorageContextFactory.InvalidateContext();
        RefreshTopItems();
    }

    // Generous relative to any real eat/drink animation (a few seconds) — this only exists to catch
    // the busy flag getting stranded true by an exception in a later, unrelated coroutine tick that
    // we have no way to try/catch around (see TryUseSlot). Checked every frame from the window.
    private const float STUCK_USE_TIMEOUT_SECONDS = 10f;
    private float? _pendingUseStartedAt;

    private void MarkUsePending()
    {
        _pendingUseStartedAt = Time.time;
    }

    /// <summary>
    /// Recovers from a soft lock if a use we triggered never cleared xui.IsUsingItemActionEntryUse
    /// within a generous timeout — see the comment in TryUseSlot for why this can happen and why it
    /// can't be caught with a try/catch at the call site.
    /// </summary>
    internal void CheckStuckUseWatchdog()
    {
        if (_pendingUseStartedAt == null)
        {
            return;
        }

        if (!xui.IsUsingItemActionEntryUse)
        {
            _pendingUseStartedAt = null; // completed normally
            return;
        }

        if (Time.time - _pendingUseStartedAt.Value < STUCK_USE_TIMEOUT_SECONDS)
        {
            return;
        }

        ModLogger.Error($"{nameof(CheckStuckUseWatchdog)}: IsUsingItemActionEntryUse still true {STUCK_USE_TIMEOUT_SECONDS}s after a Useables window use — force-clearing to recover from a soft lock.");

        xui.IsUsingItemActionEntryUse = false;

        var controllers = GetItemStackControllers();
        for (int i = 0; i < controllers.Length; i++)
        {
            controllers[i].HiddenLock = false;
        }

        _pendingUseStartedAt = null;
        RefreshTopItems();
    }

    private static ItemActionEntryUse.ConsumeType GetConsumeTypeForSlot(int slotIndex, int itemType)
    {
        if (slotIndex < ROW_SIZE)
        {
            return ItemActionEntryUse.ConsumeType.Heal;
        }

        return UseableItemStore.IsDrinkItem(itemType) ? ItemActionEntryUse.ConsumeType.Drink : ItemActionEntryUse.ConsumeType.Eat;
    }

    /// <summary>
    /// Mirrors ItemActionEntryUse.RefreshEnabled's gates, since we're bypassing the vanilla
    /// click/enable pipeline entirely (our cells are locked) and calling OnActivated directly.
    /// </summary>
    private bool CanUseNow(ItemActionEntryUse.ConsumeType consumeType)
    {
        var entityPlayer = xui.playerUI.entityPlayer;
        if (entityPlayer == null)
        {
            return false;
        }

        if (entityPlayer.AttachedToEntity)
        {
            GameManager.ShowTooltip(entityPlayer, Localization.Get("ttCannotUseWhileOnVehicle"));
            return false;
        }

        if (entityPlayer.inventory.IsHoldingItemActionRunning() || xui.IsUsingItemActionEntryUse)
        {
            GameManager.ShowTooltip(entityPlayer, Localization.Get("isBusy"));
            return false;
        }

        if (XUiC_AssembleWindowGroup.GetWindowGroup(xui).IsOpen)
        {
            GameManager.ShowTooltip(entityPlayer, Localization.Get("ttCannotUseWhileAssembling"));
            return false;
        }

        if (consumeType == ItemActionEntryUse.ConsumeType.Drink && XUiM_Player.GetWaterPercent(entityPlayer) >= 1f)
        {
            GameManager.ShowTooltip(entityPlayer, Localization.Get("notThirsty"));
            return false;
        }

        if (consumeType == ItemActionEntryUse.ConsumeType.Eat && XUiM_Player.GetFoodPercent(entityPlayer) >= 1f)
        {
            GameManager.ShowTooltip(entityPlayer, Localization.Get("notHungry"));
            return false;
        }

        return true;
    }

    private static bool RequiresPrompt(ItemValue itemValue)
    {
        var actions = itemValue.ItemClass?.Actions;
        if (actions == null)
        {
            return false;
        }

        foreach (var action in actions)
        {
            if (action is ItemActionEat itemActionEat)
            {
                return itemActionEat.UsePrompt;
            }
        }

        return false;
    }
}
