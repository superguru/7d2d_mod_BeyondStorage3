using System;
using System.Collections.Generic;
using BeyondStorage.Infrastructure;

namespace BeyondStorage.Data;

/// <summary>
/// Classifies item types into Heal / Food / Drink categories for the Useables window, and scores
/// each by how much it actually helps the player. Classification is based on the item's Tags
/// property ("medical", "food", "drinks"), not on ItemActionEntryUse.ConsumeType — vanilla's
/// XUiC_ItemActionList.AddActionActions assigns ConsumeType.Heal to every ItemActionEat item
/// regardless of whether it's medical, food, or drink, so that enum can't be used to tell them
/// apart. Tags are the actual signal items.xml uses.
/// Built once per game session from ItemClass.list, since items.xml is only loaded once.
/// </summary>
public static class UseableItemStore
{
    private static readonly HashSet<int> s_healItemTypes = [];
    private static readonly HashSet<int> s_foodItemTypes = [];
    private static readonly HashSet<int> s_drinkItemTypes = [];

    // Net health change from using the item once (from its "foodHealthAmount" display value, or a
    // fallback sum of ModifyStats "Health" triggered effects — see ComputeHealthEffectValue).
    // Populated for both heal and food/drink items, since it's used as the heal row's primary score
    // and the food/drink row's secondary tiebreaker.
    private static readonly Dictionary<int, float> s_healthScore = [];

    // Net "$foodAmountAdd"/"$waterAmountAdd" change from using the item once (see
    // ComputeNutritionScore). Only populated for food/drink items.
    private static readonly Dictionary<int, float> s_nutritionScore = [];

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
    /// Item types tagged "food" that have a usable eat-style action. Items tagged both "food" and
    /// "drinks" are classified here, not in <see cref="DrinkItemTypes"/>.
    /// </summary>
    public static IReadOnlyCollection<int> FoodItemTypes
    {
        get
        {
            EnsureBuilt();
            return s_foodItemTypes;
        }
    }

    /// <summary>
    /// Item types tagged "drinks" (and not also "food") that have a usable eat-style action.
    /// </summary>
    public static IReadOnlyCollection<int> DrinkItemTypes
    {
        get
        {
            EnsureBuilt();
            return s_drinkItemTypes;
        }
    }

    public static bool IsHealItem(int itemType)
    {
        EnsureBuilt();
        return s_healItemTypes.Contains(itemType);
    }

    public static bool IsFoodItem(int itemType)
    {
        EnsureBuilt();
        return s_foodItemTypes.Contains(itemType);
    }

    public static bool IsDrinkItem(int itemType)
    {
        EnsureBuilt();
        return s_drinkItemTypes.Contains(itemType);
    }

    /// <summary>
    /// Score for ranking the Heal row: (heal amount, unused). Count is the natural tiebreaker
    /// applied by StorageSourceItemDataStore.GetTopItemsByScore when heal amount ties (e.g. two
    /// unscored items, or a genuine tie), matching "highest buff, then highest count".
    /// </summary>
    public static (float Primary, float Secondary) GetHealScore(int itemType)
    {
        EnsureBuilt();
        var health = s_healthScore.TryGetValue(itemType, out var h) ? h : 0f;
        return (health, 0f);
    }

    /// <summary>
    /// Score for ranking the Food/Drink row: (nutrition value, net health effect). Ranking by this
    /// instead of raw count means a real meal outranks something abundant but harmful like rotting
    /// flesh (+1 food, -3 health) despite it often being the most plentiful item in storage.
    /// </summary>
    public static (float Primary, float Secondary) GetNutritionScore(int itemType)
    {
        EnsureBuilt();
        var nutrition = s_nutritionScore.TryGetValue(itemType, out var n) ? n : 0f;
        var health = s_healthScore.TryGetValue(itemType, out var h) ? h : 0f;
        return (nutrition, health);
    }

    /// <summary>
    /// Populates the Heal / Food / Drink sets and their scores by resolving every registered item
    /// type once. ItemClass.list is only assigned once per game session (items.xml load), so a
    /// one-time build is safe to cache, mirroring ItemClassCache.EnsureUseageIndexBuilt.
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

            // Only items with a usable eat-style action are eligible for any row
            if (ItemClassCache.LookupItemUseageType(itemType) == ItemActionEntryUse.ConsumeType.None)
            {
                continue;
            }

            bool isFood = itemClass.HasAnyTags(s_foodTag);
            bool isDrink = itemClass.HasAnyTags(s_drinksTag);

            if (isFood || isDrink)
            {
                (isFood ? s_foodItemTypes : s_drinkItemTypes).Add(itemType);
                s_nutritionScore[itemType] = ComputeNutritionScore(itemClass);
                s_healthScore[itemType] = ComputeHealthEffectValue(itemClass);
                continue;
            }

            if (itemClass.HasAnyTags(s_medicalTag))
            {
                s_healItemTypes.Add(itemType);
                s_healthScore[itemType] = ComputeHealthEffectValue(itemClass);
            }
        }

        ModLogger.Info($"{nameof(UseableItemStore)}: Classified {s_healItemTypes.Count} heal, {s_foodItemTypes.Count} food, {s_drinkItemTypes.Count} drink item types");
    }

    /// <summary>
    /// Sums the net food/water added from the item's "primary action end" triggered effects — i.e.
    /// what actually happens when the player eats/drinks it once. These effect definitions are
    /// static per item type (parsed once from items.xml), so this is safe to compute during the
    /// one-time build. Effects whose value is a dynamic CVar reference (e.g. value="@$SomeCVar")
    /// are skipped rather than guessed at, since their true value depends on player/runtime state
    /// we don't have here.
    /// </summary>
    private static float ComputeNutritionScore(ItemClass itemClass)
    {
        float nutrition = 0f;

        foreach (var action in GetPrimaryActionEndEffects(itemClass))
        {
            if (action is MinEventActionModifyCVar { cvarRef: false } modifyCVar && IsFoodOrWaterCVar(modifyCVar.cvarName))
            {
                nutrition += modifyCVar.GetValueForDisplay();
            }
        }

        return nutrition;
    }

    /// <summary>
    /// Net health change from using the item once. Prefers the "foodHealthAmount" display value —
    /// the same designer-authored stat items.xml/ui_display.xml show in the tooltip for both food
    /// and medical items (e.g. rotting flesh: -3, a first aid kit: +180) — falling back to summing
    /// ModifyStats "Health" triggered effects for items with no such display value (e.g. some
    /// medical items that cure a condition rather than restoring HP directly, which then score 0
    /// here and fall back to being ranked by count via GetTopItemsByScore's tiebreak).
    /// </summary>
    private static float ComputeHealthEffectValue(ItemClass itemClass)
    {
        var effectGroups = itemClass.Effects?.EffectGroups;
        if (effectGroups == null)
        {
            return 0f;
        }

        foreach (var group in effectGroups)
        {
            if (group.EffectDisplayValues != null && group.EffectDisplayValues.TryGetValue("foodHealthAmount", out var displayValue))
            {
                return displayValue.GetValue(0);
            }
        }

        float health = 0f;
        foreach (var action in GetPrimaryActionEndEffects(itemClass))
        {
            if (action is MinEventActionModifyStats { cvarRef: false, statName: "health" } modifyStats)
            {
                health += GetStatDelta(modifyStats);
            }
        }

        return health;
    }

    private static IEnumerable<MinEventActionBase> GetPrimaryActionEndEffects(ItemClass itemClass)
    {
        var effectGroups = itemClass.Effects?.EffectGroups;
        if (effectGroups == null)
        {
            yield break;
        }

        foreach (var group in effectGroups)
        {
            foreach (var action in group.GetTriggeredEffects(MinEventTypes.onSelfPrimaryActionEnd))
            {
                yield return action;
            }
        }
    }

    private static bool IsFoodOrWaterCVar(string cvarName)
    {
        return string.Equals(cvarName, "$foodAmountAdd", StringComparison.OrdinalIgnoreCase)
            || string.Equals(cvarName, "$waterAmountAdd", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Only "add"/"subtract" have an unambiguous delta — "set" replaces the stat outright rather
    /// than changing it by a fixed amount, so it can't be summed the same way and is treated as 0.
    /// </summary>
    private static float GetStatDelta(MinEventActionModifyStats modifyStats)
    {
        return modifyStats.operation switch
        {
            MinEventActionModifyStats.OperationTypes.add => modifyStats.value,
            MinEventActionModifyStats.OperationTypes.subtract => -modifyStats.value,
            _ => 0f,
        };
    }
}
