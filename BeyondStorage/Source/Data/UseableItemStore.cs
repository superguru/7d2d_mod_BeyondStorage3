using System.Collections.Generic;
using BeyondStorage.Infrastructure;

namespace BeyondStorage.Data;

/// <summary>
/// Classifies item types into Heal / Food-Drink categories for the Useables window.
/// Classification is based on the item's Tags property ("medical", "food", "drinks"), not on
/// ItemActionEntryUse.ConsumeType — vanilla's XUiC_ItemActionList.AddActionActions assigns
/// ConsumeType.Heal to every ItemActionEat item regardless of whether it's medical, food, or
/// drink, so that enum can't be used to tell them apart. Tags are the actual signal items.xml uses.
/// Built once per game session from ItemClass.list, since items.xml is only loaded once.
/// </summary>
public static class UseableItemStore
{
    private static readonly HashSet<int> s_healItemTypes = [];
    private static readonly HashSet<int> s_foodDrinkItemTypes = [];
    private static bool s_built;

    private static readonly FastTags<TagGroup.Global> s_medicalTag = FastTags<TagGroup.Global>.Parse("medical");
    private static readonly FastTags<TagGroup.Global> s_foodTag = FastTags<TagGroup.Global>.Parse("food");
    private static readonly FastTags<TagGroup.Global> s_drinksTag = FastTags<TagGroup.Global>.Parse("drinks");

    /// <summary>
    /// Item types tagged "medical" (and not also "food"/"drinks") that have a usable eat-style action.
    /// </summary>
    public static IReadOnlyCollection<int> HealItemTypes
    {
        get
        {
            EnsureBuilt();
            return s_healItemTypes;
        }
    }

    /// <summary>
    /// Item types tagged "food" and/or "drinks" that have a usable eat-style action.
    /// Items tagged both "medical" and "food"/"drinks" (e.g. a healing stew) are classified here,
    /// not in <see cref="HealItemTypes"/>, since they're primarily consumables.
    /// </summary>
    public static IReadOnlyCollection<int> FoodDrinkItemTypes
    {
        get
        {
            EnsureBuilt();
            return s_foodDrinkItemTypes;
        }
    }

    public static bool IsHealItem(int itemType)
    {
        EnsureBuilt();
        return s_healItemTypes.Contains(itemType);
    }

    public static bool IsFoodOrDrinkItem(int itemType)
    {
        EnsureBuilt();
        return s_foodDrinkItemTypes.Contains(itemType);
    }

    /// <summary>
    /// Populates s_healItemTypes and s_foodDrinkItemTypes by resolving every registered item type
    /// once. ItemClass.list is only assigned once per game session (items.xml load), so a one-time
    /// build is safe to cache, mirroring ItemClassCache.EnsureUseageIndexBuilt.
    /// </summary>
    private static void EnsureBuilt()
    {
        if (s_built)
        {
            return;
        }
        s_built = true;

        var itemClasses = ItemClass.list;
        if (itemClasses == null)
        {
            return;
        }

        for (int itemType = 1; itemType < itemClasses.Length; itemType++)
        {
            var itemClass = itemClasses[itemType];
            if (itemClass == null)
            {
                continue;
            }

            // Only items with a usable eat-style action are eligible for either row
            if (ItemClassCache.LookupItemUseageType(itemType) == ItemActionEntryUse.ConsumeType.None)
            {
                continue;
            }

            bool isFoodOrDrink = itemClass.HasAnyTags(s_foodTag) || itemClass.HasAnyTags(s_drinksTag);
            if (isFoodOrDrink)
            {
                s_foodDrinkItemTypes.Add(itemType);
                continue;
            }

            if (itemClass.HasAnyTags(s_medicalTag))
            {
                s_healItemTypes.Add(itemType);
            }
        }

        ModLogger.Info($"{nameof(UseableItemStore)}: Classified {s_healItemTypes.Count} heal item types and {s_foodDrinkItemTypes.Count} food/drink item types");
    }
}
