using System.Collections.Generic;
using BeyondStorage.Data;
using BeyondStorage.Infrastructure;
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
    /// Cells here are synthetic display stacks aggregated across possibly many storage sources, not
    /// a reference to any single real slot, so picking one up would hand the player a free copy of
    /// a real item without ever removing it from storage — a duplication bug, not just a UX wrinkle.
    /// IsLocked gates the entire mouse/gamepad interaction block on XUiC_ItemStack (click, drag,
    /// swap, partial-stack pickup), unlike AllowDropping which only gates drops landing on the slot —
    /// AllowDropping is set too as a second layer, but IsLocked is what actually prevents pickup.
    /// Re-applied after every refresh since SetStacks recreates the underlying ItemStack.
    /// </summary>
    private void LockCells()
    {
        var controllers = GetItemStackControllers();
        for (int i = 0; i < controllers.Length; i++)
        {
            controllers[i].AllowDropping = false;
            controllers[i].IsLocked = true;
        }
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public override void UpdateBackend(ItemStack[] stackList)
    {
        ModLogger.DebugLog($"UpdateBackend: stackList.Length={stackList?.Length}");
        base.UpdateBackend(stackList);  // TODO: Should we be doing this?
        windowGroup.Controller.SetAllChildrenDirty();
    }
}
