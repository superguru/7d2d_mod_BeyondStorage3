using System.Collections.Generic;
using BeyondStorage.Infrastructure;

namespace BeyondStorage.Data;

public static class ItemClassCache
{
    private static readonly Dictionary<int, string> s_itemTypeNames = [];
    private static readonly Dictionary<int, int> s_itemMaxStackSizes = [];
    private static readonly Dictionary<int, ItemActionEntryUse.ConsumeType> s_itemUseageTypes = [];
    private static readonly Dictionary<ItemActionEntryUse.ConsumeType, HashSet<int>> s_useageTypeIndex = [];
    private static bool s_useageIndexBuilt;

    private static int s_totalMaxStackSize = 0;

    public static string LookupItemName(int itemType)
    {
        const string d_MethodName = nameof(LookupItemName);

        if (itemType < UniqueItemTypes.WILDCARD)
        {
            var invalidResult = $"Invalid Item Type ({itemType})";
            ModLogger.DebugLog($"{d_MethodName}({itemType}) | Invalid item type, returning: {invalidResult}");
            return invalidResult;
        }

        if (itemType == UniqueItemTypes.WILDCARD)
        {
            return "*";  // Don't cache constants
        }

        if (itemType == UniqueItemTypes.EMPTY)
        {
            return "null";  // Don't cache constants, use consistent return value
        }

        if (s_itemTypeNames.TryGetValue(itemType, out var name))
        {
            return name;
        }

        name = ResolveItemName(itemType);
        s_itemTypeNames[itemType] = name;
        return name;
    }

    /// <summary>
    /// Resolves the name for a given item type by looking up the ItemClass and handling fallbacks.
    /// </summary>
    /// <param name="itemType">The item type to resolve</param>
    /// <returns>The resolved item name or a fallback name if not found</returns>
    private static string ResolveItemName(int itemType)
    {
        // Lookup the item class and get its name
        var itemClass = ItemClass.GetForId(itemType);
        var itemName = itemClass?.GetItemName();

        // Handle null or empty item names more robustly
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return $"Unknown Item Type {itemType}";
        }
        else
        {
            return itemName;
        }
    }

    public static string LookupItemLocalisedName(int itemType)
    {
        const string d_MethodName = nameof(LookupItemLocalisedName);

        var itemName = LookupItemName(itemType);
        var localisedName = GameTools.GetLocalisedValue(d_MethodName, itemName);
#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName} {itemType} -> {itemName} -> {localisedName}");
#endif
        return localisedName;
    }

    public static string LookupItemName(ItemValue itemValue)
    {
        return LookupItemName(itemValue?.type ?? UniqueItemTypes.EMPTY);
    }

    public static string LookupItemName(ItemStack itemStack)
    {
        return LookupItemName(itemStack?.itemValue);
    }

    public static string LookupItemLocalisedName(ItemValue itemValue)
    {
        return LookupItemLocalisedName(itemValue?.type ?? UniqueItemTypes.EMPTY);
    }

    public static string LookupItemLocalisedName(ItemStack itemStack)
    {
        return LookupItemLocalisedName(itemStack?.itemValue);
    }

    public static ItemActionEntryUse.ConsumeType LookupItemUseageType(int itemType)
    {
        if (itemType <= UniqueItemTypes.EMPTY)
        {
            return ItemActionEntryUse.ConsumeType.None;  // Don't cache constants/invalid types (covers WILDCARD, EMPTY, and < WILDCARD)
        }

        if (s_itemUseageTypes.TryGetValue(itemType, out var consumeType))
        {
            return consumeType;
        }

        consumeType = ResolveItemUseageType(itemType);
        s_itemUseageTypes[itemType] = consumeType;
        return consumeType;
    }

    /// <summary>
    /// Resolves the use-action classification for a given item type by inspecting its ItemClass.Actions.
    /// Mirrors the type mapping in XUiC_ItemActionList.AddActionActions, minus the stack-location-dependent
    /// checks (UsePrompt/backpack-toolbelt), since this lookup has no UI stack context.
    /// </summary>
    /// <param name="itemType">The item type to resolve</param>
    /// <returns>The resolved ConsumeType, or None if the item has no matching action</returns>
    private static ItemActionEntryUse.ConsumeType ResolveItemUseageType(int itemType)
    {
        var actions = ItemClass.GetForId(itemType)?.Actions;
        if (actions == null)
        {
            return ItemActionEntryUse.ConsumeType.None;
        }

        foreach (var action in actions)
        {
            switch (action)
            {
                case ItemActionEat:
                    return ItemActionEntryUse.ConsumeType.Heal;
                case ItemActionLearnRecipe:
                case ItemActionGainSkill:
                    return ItemActionEntryUse.ConsumeType.Read;
                case ItemActionQuest:
                    return ItemActionEntryUse.ConsumeType.Quest;
                case ItemActionOpenBundle:
                case ItemActionOpenLootBundle:
                    return ItemActionEntryUse.ConsumeType.Open;
            }
        }

        return ItemActionEntryUse.ConsumeType.None;
    }

    public static ItemActionEntryUse.ConsumeType LookupItemUseageType(ItemValue itemValue)
    {
        return LookupItemUseageType(itemValue?.type ?? UniqueItemTypes.EMPTY);
    }

    public static ItemActionEntryUse.ConsumeType LookupItemUseageType(ItemStack itemStack)
    {
        return LookupItemUseageType(itemStack?.itemValue);
    }

    /// <summary>
    /// Returns every item type whose useage classification matches any of the given ConsumeTypes,
    /// e.g. GetItemTypesWithUseageType(ConsumeType.Heal, ConsumeType.Quest).
    /// Backed by a lazily-built reverse index over ItemClass.list, built once on first use.
    /// </summary>
    public static IReadOnlyCollection<int> GetItemTypesWithUseageType(params ItemActionEntryUse.ConsumeType[] consumeTypes)
    {
        EnsureUseageIndexBuilt();

        if (consumeTypes == null || consumeTypes.Length == 0)
        {
            return [];
        }

        if (consumeTypes.Length == 1)
        {
            return s_useageTypeIndex.TryGetValue(consumeTypes[0], out var single) ? single : [];
        }

        var result = new HashSet<int>();
        foreach (var consumeType in consumeTypes)
        {
            if (s_useageTypeIndex.TryGetValue(consumeType, out var itemTypes))
            {
                result.UnionWith(itemTypes);
            }
        }
        return result;
    }

    /// <summary>
    /// Populates s_useageTypeIndex by resolving every registered item type once. ItemClass.list is
    /// only assigned once per game session (items.xml load), so a one-time build is safe to cache.
    /// </summary>
    private static void EnsureUseageIndexBuilt()
    {
        if (s_useageIndexBuilt)
        {
            return;
        }
        s_useageIndexBuilt = true;

        var itemClasses = ItemClass.list;
        if (itemClasses == null)
        {
            return;
        }

        for (int itemType = 1; itemType < itemClasses.Length; itemType++)
        {
            if (itemClasses[itemType] == null)
            {
                continue;
            }

            var consumeType = LookupItemUseageType(itemType);
            if (consumeType == ItemActionEntryUse.ConsumeType.None)
            {
                continue;
            }

            if (!s_useageTypeIndex.TryGetValue(consumeType, out var itemTypes))
            {
                itemTypes = [];
                s_useageTypeIndex[consumeType] = itemTypes;
            }
            itemTypes.Add(itemType);
        }
    }

    public static int LookupMaxStackSize(int itemType)
    {
        const string d_MethodName = nameof(LookupMaxStackSize);

        if (itemType < UniqueItemTypes.WILDCARD)
        {
            ModLogger.DebugLog($"{d_MethodName}({itemType}) | Invalid item type, returning 0");
            return 0;  // Don't cache constants
        }

        if (itemType == UniqueItemTypes.WILDCARD)
        {
            return 0;  // Don't cache constants
        }

        if (itemType == UniqueItemTypes.EMPTY)
        {
            return 0;  // Don't cache constants
        }

        if (s_itemMaxStackSizes.TryGetValue(itemType, out var maxStackSize))
        {
            return maxStackSize;
        }

        maxStackSize = ResolveMaxStackSize(itemType);
        s_itemMaxStackSizes[itemType] = maxStackSize;

        if (maxStackSize > 0)
        {
            s_totalMaxStackSize = maxStackSize;
        }

        return maxStackSize;
    }

    /// <summary>
    /// Resolves the max stack size for a given item type by looking up the ItemClass and handling fallbacks.
    /// </summary>
    /// <param name="itemType">The item type to resolve</param>
    /// <returns>The resolved max stack size, or 0 if not found</returns>
    private static int ResolveMaxStackSize(int itemType)
    {
        ItemClass itemClass = itemType <= 0 ? null : ItemClass.GetForId(itemType);
        if (itemClass == null)
        {
            return 0;
        }

        return itemClass.MaxCount;
    }

    public static int LookupMaxStackSize(ItemValue itemValue)
    {
        return LookupMaxStackSize(itemValue?.type ?? UniqueItemTypes.EMPTY);
    }

    public static int LookupMaxStackSize(ItemStack itemStack)
    {
        return LookupMaxStackSize(itemStack?.itemValue);
    }
}
