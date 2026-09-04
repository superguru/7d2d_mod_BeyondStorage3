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

    // Health deficit below which the heal slot is suppressed (the player is effectively at full
    // health). 0 means any missing HP shows a heal slot; raise it if tiny wounds shouldn't.
    private const float HEAL_SLOT_MIN_DEFICIT = 0f;

    // How far the health deficit must move before the heal row re-ranks (see _healFitDeficit).
    private const float HEAL_REORDER_THRESHOLD = 10f;

    // Deficit last used to rank the heal row. Hysteresis keeps the ordering stable while a healing
    // buff slowly recovers HP; the row only re-ranks once the real deficit moves by more than
    // HEAL_REORDER_THRESHOLD.
    private float _healFitDeficit = -1f;

    // Item types shown in the heal row last refresh (in order). Used to keep the ordering stable
    // when the same items are still shown, so marginal score changes (e.g. two cures competing for
    // the same debuff) don't make items swap between refreshes.
    private readonly List<int> _previousHealRowTypes = new(ROW_SIZE);

    // Item types shown in the food/drink row last refresh (in order) — same stability purpose.
    private readonly List<int> _previousFoodDrinkRowTypes = new(ROW_SIZE);

    public override void OnOpen()
    {
        base.OnOpen();
        RefreshGridItems();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    /// <summary>
    /// Repopulates the grid: row 0 (slots 1-3) with a heal item chosen to fit the player's current
    /// health deficit plus up to 2 items that cure debuffs the player currently has (see
    /// ComposeHealRow); row 1 (slots 4-6) with 1 food + 2 drinks (falling back to whichever category
    /// has items if the other is empty), each ranked by nutrition value then net health effect. Cells
    /// are display-only synthetic stacks, not live references to a storage slot — see
    /// StorageSourceItemDataStore.GetTopItemsByScore for how the ranking avoids re-walking storage.
    /// </summary>
    internal void RefreshGridItems()
    {
        const string d_MethodName = nameof(RefreshGridItems);

        if (!ValidationHelper.ValidateStorageContext(d_MethodName, out var context))
        {
            SetStacks(BuildEmptySlots());
            return;
        }

        if (!context.Config.ShowUseables)
        {
            SetStacks(BuildEmptySlots());
            return;
        }

        // Top means the highest ranked, based on various conditions such as buffs, debuffs, item count
        var player = context.Player;
        float healableDeficit = 0f;
        float woundedDeficit = 0f;
        if (player != null)
        {
            healableDeficit = Mathf.Max(0f, player.Stats.Health.ModifiedMax - player.Stats.Health.Value);
            woundedDeficit = Mathf.Max(0f, player.Stats.Health.Max - player.Stats.Health.Value);
        }

        // Hysteresis on the healable deficit (which heal item to pick) so a healing buff slowly
        // recovering HP doesn't make the top-row items swap every refresh.
        if (_healFitDeficit < 0f || Mathf.Abs(healableDeficit - _healFitDeficit) > HEAL_REORDER_THRESHOLD)
        {
            _healFitDeficit = healableDeficit;
        }

        var healRanked = context.GetTopUseableItemsByScore(
            itemType => UseableItemStore.IsHealItem(itemType) && UseableItemStore.GetHealAmount(itemType) > 0f,
            itemType => UseableItemStore.GetContextualHealScore(itemType, _healFitDeficit),
            ROW_SIZE);
        var cureRanked = context.GetTopUseableItemsByScore(
            itemType => UseableItemStore.CuresAnyActiveDebuff(itemType, player),
            itemType => UseableItemStore.GetCureScore(itemType, player),
            ROW_SIZE);
        // needsHeal is decided against the base-max deficit so buffs that modify max health (e.g.
        // infection) don't make the heal slot blink in and out.
        var healRow = StabiliseOrder(ComposeHealRow(healRanked, cureRanked, woundedDeficit), _previousHealRowTypes);

        // Items already shown in the heal row (e.g. honey pulled up as a cure) must not repeat in
        // the food/drink row.
        var usedInHealRow = new HashSet<int>();
        foreach (var item in healRow)
        {
            usedInHealRow.Add(item.ItemType);
        }

        var foodTop = context.GetTopUseableItemsByScore(UseableItemStore.IsFoodItem, UseableItemStore.GetNutritionScore, ROW_SIZE);
        var drinkTop = context.GetTopUseableItemsByScore(UseableItemStore.IsDrinkItem, UseableItemStore.GetNutritionScore, ROW_SIZE);
        var foodDrinkRow = StabiliseOrder(ComposeFoodDrinkRow(foodTop, drinkTop, usedInHealRow), _previousFoodDrinkRowTypes);

        var stacks = BuildEmptySlots();
        FillRow(stacks, rowStart: 0, topItems: healRow);
        FillRow(stacks, rowStart: ROW_SIZE, topItems: foodDrinkRow);

        SetStacks(stacks);
        LockCells();
    }

    /// <summary>
    /// Fills the food/drink row: up to <see cref="FOOD_QUOTA"/> food + <see cref="DRINK_QUOTA"/>
    /// drinks, then backfills any remaining slots from whichever list still has items (food first).
    /// This naturally degrades to "just food" when there are no drinks, or "just drinks" when there
    /// is no food, while otherwise preserving each list's own nutrition-based ranking. Item types
    /// already shown in the heal row are excluded so a food/drink cure item (e.g. honey) doesn't
    /// appear in both rows.
    /// </summary>
    private static List<(int ItemType, int Count)> ComposeFoodDrinkRow(
        IReadOnlyList<(int ItemType, int Count)> foodTop,
        IReadOnlyList<(int ItemType, int Count)> drinkTop,
        ISet<int> excludedItemTypes)
    {
        var food = FilterExcluded(foodTop, excludedItemTypes);
        var drink = FilterExcluded(drinkTop, excludedItemTypes);

        var result = new List<(int ItemType, int Count)>(ROW_SIZE);
        int foodIndex = 0;
        int drinkIndex = 0;

        for (int i = 0; i < FOOD_QUOTA && foodIndex < food.Count; i++)
        {
            result.Add(food[foodIndex++]);
        }

        for (int i = 0; i < DRINK_QUOTA && drinkIndex < drink.Count; i++)
        {
            result.Add(drink[drinkIndex++]);
        }

        while (result.Count < ROW_SIZE && (foodIndex < food.Count || drinkIndex < drink.Count))
        {
            if (foodIndex < food.Count)
            {
                result.Add(food[foodIndex++]);
            }
            else
            {
                result.Add(drink[drinkIndex++]);
            }
        }

        return result;
    }

    private static List<(int ItemType, int Count)> FilterExcluded(
        IReadOnlyList<(int ItemType, int Count)> items,
        ISet<int> excludedItemTypes)
    {
        var filtered = new List<(int ItemType, int Count)>(items.Count);
        foreach (var item in items)
        {
            if (!excludedItemTypes.Contains(item.ItemType))
            {
                filtered.Add(item);
            }
        }
        return filtered;
    }

    private static bool SameItemTypeSet(List<(int ItemType, int Count)> row, List<int> previousTypes)
    {
        if (row.Count != previousTypes.Count)
        {
            return false;
        }

        foreach (var item in row)
        {
            if (!previousTypes.Contains(item.ItemType))
            {
                return false;
            }
        }

        return true;
    }

    private static List<(int ItemType, int Count)> ReorderToPrevious(List<(int ItemType, int Count)> row, List<int> previousTypes)
    {
        var reordered = new List<(int ItemType, int Count)>(row.Count);
        var remaining = new List<(int ItemType, int Count)>(row);

        foreach (var prevType in previousTypes)
        {
            for (int i = 0; i < remaining.Count; i++)
            {
                if (remaining[i].ItemType == prevType)
                {
                    reordered.Add(remaining[i]);
                    remaining.RemoveAt(i);
                    break;
                }
            }
        }

        reordered.AddRange(remaining);
        return reordered;
    }

    /// <summary>
    /// Reuses the previous ordering when the same item types are still shown (irrespective of their
    /// fresh order), then records the new order for next refresh. This keeps a row stable across
    /// refreshes so marginal score changes don't make items swap.
    /// </summary>
    private static List<(int ItemType, int Count)> StabiliseOrder(
        List<(int ItemType, int Count)> row,
        List<int> previousTypes)
    {
        if (previousTypes.Count > 0 && SameItemTypeSet(row, previousTypes))
        {
            row = ReorderToPrevious(row, previousTypes);
        }

        previousTypes.Clear();
        foreach (var item in row)
        {
            previousTypes.Add(item.ItemType);
        }

        return row;
    }

    /// <summary>
    /// Composes the heal row (slots 1-3) as 1 heal + 2 cure per the plan: heal-first with a
    /// conditional heal slot. When wounded the best heal-fit item goes first and the remaining slots
    /// fill with cures (backfilled with more heals if there aren't enough cures); at full HP cures
    /// take the whole row; at full HP with no debuffs the smallest heal is shown so the row isn't
    /// empty. The same item is never shown twice.
    /// </summary>
    private static List<(int ItemType, int Count)> ComposeHealRow(
        IReadOnlyList<(int ItemType, int Count)> healRanked,
        IReadOnlyList<(int ItemType, int Count)> cureRanked,
        float healthDeficit)
    {
        var result = new List<(int ItemType, int Count)>(ROW_SIZE);
        var used = new HashSet<int>();

        bool needsHeal = healthDeficit > HEAL_SLOT_MIN_DEFICIT;

        if (needsHeal && healRanked.Count > 0)
        {
            result.Add(healRanked[0]);
            used.Add(healRanked[0].ItemType);
        }

        FillWithUnique(result, used, cureRanked);

        if (needsHeal)
        {
            FillWithUnique(result, used, healRanked);
        }
        else if (result.Count == 0 && healRanked.Count > 0)
        {
            result.Add(healRanked[0]);
        }

        return result;
    }

    /// <summary>
    /// Appends items from <paramref name="items"/> until the row is full, skipping any item type
    /// already present in <paramref name="result"/> so the same item never appears twice.
    /// </summary>
    private static void FillWithUnique(
        List<(int ItemType, int Count)> result,
        HashSet<int> used,
        IReadOnlyList<(int ItemType, int Count)> items)
    {
        foreach (var item in items)
        {
            if (result.Count >= ROW_SIZE)
            {
                break;
            }

            if (used.Add(item.ItemType))
            {
                result.Add(item);
            }
        }
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
            var newStack = new ItemStack(new ItemValue(itemType), count);

            stacks[rowStart + i] = newStack;
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

        if (!CanUseNow(itemValue))
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
            RefreshGridItems();
            return;
        }

        // Leave the cell showing its real (pre-use) count so vanilla's own decrement lands on the
        // correct post-use count: OnActivated clones the cell stack as originalStack, then
        // non-animated items decrement the cell in place via ExecuteInstantAction and animated
        // items set it to originalStack.count - 1. Replacing it with a 1-count clone here made the
        // animation coroutine wipe the cell to empty (a visible flash) because originalStack.count
        // was 1 instead of the real count.

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
        RefreshGridItems();
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
        RefreshGridItems();
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
    /// Food and drink are intentionally allowed regardless of hunger/thirst, so there is no
    /// fullness gate here — only the item's own ExecutionRequirements (e.g. a First Aid Kit
    /// requiring health below 100%) block a use, and that is checked before removal so a failed
    /// use never wastes the item.
    /// </summary>
    private bool CanUseNow(ItemValue itemValue)
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

        // ItemActionEntryUse.OnActivated checks this internally (hardcoded to action index 0)
        // AFTER we'd have already removed the item from storage — e.g. a First Aid Kit's own
        // requirement (health must be below 100%) fails silently in there with zero effect
        // applied. Checking it here, before removal, is what actually prevents losing the item.
        if (!itemValue.ItemClass.CanExecuteAction(0, entityPlayer, itemValue))
        {
            GameManager.ShowTooltip(entityPlayer, Localization.Get("ttCannotUseAtThisTime"), string.Empty, "ui_denied");
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
