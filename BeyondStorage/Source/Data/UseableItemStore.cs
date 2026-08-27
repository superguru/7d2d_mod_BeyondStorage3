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

    // Debuff buff names each item cures, keyed by item type. Populated for any eat item — heal,
    // food, or drink — that has a cure effect, so food/drink cures (e.g. foodHoney for infection)
    // can be pulled into the heal row when the debuff is active.
    private static readonly Dictionary<int, string[]> s_curesByItem = [];

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
    /// Raw heal amount for an item, or 0 if it doesn't restore HP (or isn't classified).
    /// </summary>
    public static float GetHealAmount(int itemType)
    {
        EnsureBuilt();
        return s_healthScore.TryGetValue(itemType, out var h) ? h : 0f;
    }

    /// <summary>
    /// Player-aware score for ranking the Heal slot: primary is how closely the item's heal amount
    /// matches the player's current health deficit (a large heal wins when badly wounded and a small
    /// heal wins when nearly full, instead of always preferring the biggest heal); secondary is the
    /// raw heal amount so a bigger heal breaks a tie.
    /// </summary>
    public static (float Primary, float Secondary) GetContextualHealScore(int itemType, float healthDeficit)
    {
        EnsureBuilt();
        var healAmount = GetHealAmount(itemType);

        // Closer to the deficit is better; a perfect match scores 0, everything else is negative.
        var fit = -Math.Abs(healAmount - healthDeficit);
        return (fit, healAmount);
    }

    /// <summary>
    /// True if the item cures at least one debuff the player currently has.
    /// </summary>
    public static bool CuresAnyActiveDebuff(int itemType, EntityPlayerLocal player)
    {
        EnsureBuilt();
        if (player?.Buffs == null || !s_curesByItem.TryGetValue(itemType, out var cures))
        {
            return false;
        }

        foreach (var buff in cures)
        {
            if (player.Buffs.HasBuff(buff))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Score for ranking cure items: primary is how many currently-active debuffs the item cures,
    /// secondary is its heal amount (a cure that also heals outranks a pure cure).
    /// </summary>
    public static (float Primary, float Secondary) GetCureScore(int itemType, EntityPlayerLocal player)
    {
        EnsureBuilt();
        var healAmount = GetHealAmount(itemType);

        int activeCures = 0;
        if (player?.Buffs != null && s_curesByItem.TryGetValue(itemType, out var cures))
        {
            foreach (var buff in cures)
            {
                if (player.Buffs.HasBuff(buff))
                {
                    activeCures++;
                }
            }
        }

        return (activeCures, healAmount);
    }

    /// <summary>
    /// Large enough that any health-debuffed item's adjusted score stays below every non-debuffed
    /// item's raw nutrition score (which items.xml keeps in the tens/low hundreds), while still
    /// preserving relative ordering *within* each tier. This ranking is expected to evolve with
    /// playtesting — tune this, or replace the tiering approach, as real items expose edge cases.
    /// </summary>
    private const float HEALTH_DEBUFF_TIER_PENALTY = 100000f;

    /// <summary>
    /// Score for ranking the Food/Drink row: (nutrition value, net health effect) — except a health
    /// debuff (e.g. foodShamSandwich: +15 food, -5 health) demotes the item into a tier below every
    /// non-debuffed food/drink outright, rather than only losing a tiebreak against similar-nutrition
    /// items. Without this, a big enough nutrition value could still outrank "this hurts you".
    /// Also means a real meal outranks something abundant but harmful like rotting flesh (+1 food,
    /// -3 health) despite it often being the most plentiful item in storage.
    /// </summary>
    public static (float Primary, float Secondary) GetNutritionScore(int itemType)
    {
        EnsureBuilt();
        var nutrition = s_nutritionScore.TryGetValue(itemType, out var n) ? n : 0f;
        var health = s_healthScore.TryGetValue(itemType, out var h) ? h : 0f;

        var primary = health < 0f ? nutrition - HEALTH_DEBUFF_TIER_PENALTY : nutrition;
        return (primary, health);
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

            // Only items with an eat-style action are eligible for any row. Tags alone aren't
            // enough: a modded item tagged "food"/"drinks"/"medical" but with a Read/Quest/Open
            // action would otherwise be classified here and then mishandled by TryUseSlot /
            // ItemActionEntryUse.OnActivated, which for Eat/Drink/Heal picks the first non-null
            // action regardless of its type. ConsumeType.Heal is what LookupItemUseageType returns
            // for ItemActionEat, so this keeps exactly the eat items.
            if (ItemClassCache.LookupItemUseageType(itemType) != ItemActionEntryUse.ConsumeType.Heal)
            {
                continue;
            }

            // Record cures for any eat item — heal, food, or drink — so food/drink cure items (e.g.
            // foodHoney for infection) can be pulled into the heal row when their debuff is active.
            var curedBuffs = ComputeCuredDebuffs(itemClass);
            if (curedBuffs.Length > 0)
            {
                s_curesByItem[itemType] = curedBuffs;
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

        ModLogger.Info($"{nameof(UseableItemStore)}: Classified {s_healItemTypes.Count} heal, {s_foodItemTypes.Count} food, {s_drinkItemTypes.Count} drink item types; {s_curesByItem.Count} cure-capable");
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
    /// Net health change from using the item once, read from the same designer-authored display
    /// values the vanilla tooltip shows. Recognises the full health vocabulary, not just
    /// "foodHealthAmount": some items expose their heal as "dInstantHealth" (e.g. drugPainkillers,
    /// +40 instant, applied via a buff) and some add a "dHealthLoss" penalty. The first effect group
    /// carrying any health signal wins — this avoids summing tiered (quality-gated) food groups,
    /// where only one tier applies per item. Falls back to summing direct ModifyStats "Health"
    /// effects for items with no health display values.
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
            if (group.EffectDisplayValues == null)
            {
                continue;
            }

            float health = 0f;
            bool found = false;

            if (group.EffectDisplayValues.TryGetValue("foodHealthAmount", out var foodHealth))
            {
                health += foodHealth.GetValue(0);
                found = true;
            }

            if (group.EffectDisplayValues.TryGetValue("dInstantHealth", out var instantHealth))
            {
                health += instantHealth.GetValue(0);
                found = true;
            }

            if (group.EffectDisplayValues.TryGetValue("dHealthLoss", out var healthLoss))
            {
                health -= healthLoss.GetValue(0);
                found = true;
            }

            if (found)
            {
                return health;
            }
        }

        float directHealth = 0f;
        foreach (var action in GetPrimaryActionEndEffects(itemClass))
        {
            if (action is MinEventActionModifyStats { cvarRef: false, statName: "health" } modifyStats)
            {
                directHealth += GetStatDelta(modifyStats);
            }
        }

        return directHealth;
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

    /// <summary>
    /// Buff names this item cures, derived from its onSelfPrimaryActionEnd effects: RemoveBuff and
    /// AddOrRemoveBuff cure their own buffs, and AddBuff cures the buffs named by its HasBuff
    /// requirements (the "treat X" pattern, e.g. aloe adding buffInjuryAbrasionTreated when
    /// buffInjuryAbrasion is present). Infection-style cures applied via a "cure progress" buff
    /// ($buffXxxAddCurePerc) aren't detected yet — that's Phase 3.
    /// </summary>
    private static string[] ComputeCuredDebuffs(ItemClass itemClass)
    {
        var cured = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var action in GetPrimaryActionEndEffects(itemClass))
        {
            if (action is MinEventActionRemoveBuff removeBuff)
            {
                AddBuffNames(removeBuff.buffNames, cured);
            }
            else if (action is MinEventActionAddOrRemoveBuff addOrRemoveBuff)
            {
                AddBuffNames(addOrRemoveBuff.buffNames, cured);
            }
            else if (action is MinEventActionAddBuff addBuff)
            {
                CollectHasBuffRequirements(addBuff.Requirements, cured);
            }
        }

        return [.. cured];
    }

    private static void AddBuffNames(string[] buffNames, HashSet<string> cured)
    {
        if (buffNames == null)
        {
            return;
        }

        foreach (var name in buffNames)
        {
            if (!string.IsNullOrEmpty(name))
            {
                cured.Add(name);
            }
        }
    }

    private static void CollectHasBuffRequirements(RequirementGroup group, HashSet<string> cured)
    {
        if (group == null)
        {
            return;
        }

        if (group.reqs != null)
        {
            foreach (var req in group.reqs)
            {
                if (req is HasBuff hasBuff)
                {
                    AddBuffNames(hasBuff.buffNames, cured);
                }
            }
        }

        if (group.groups != null)
        {
            foreach (var subGroup in group.groups)
            {
                CollectHasBuffRequirements(subGroup, cured);
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
