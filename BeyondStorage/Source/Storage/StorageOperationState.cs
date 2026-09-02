using System;
using System.Collections.Generic;
using System.Linq;
using BeyondStorage.Data;
using BeyondStorage.Storage.SmartSorting;

namespace BeyondStorage.Storage;

/// <summary>
/// Tracks the state of smart storage operations (push/pull), including items transferred and containers affected.
/// </summary>
internal class StorageOperationState
{

    private readonly HashSet<StorageTargetAdapter> _affectedStorages = [];
    private readonly HashSet<ItemStack> _affectedStacks = new(ItemStackReferenceComparer.Instance);
    private readonly HashSet<int> _uniqueItems = [];

    /// <summary>
    /// Gets the name of the master storage involved in this operation.
    /// </summary>
    public string MasterStorageName
    {
        get;
    }

    /// <summary>
    /// Gets the type of transfer operation being performed.
    /// </summary>
    public SmartTransferOperation Operation
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageOperationState"/> class.
    /// </summary>
    /// <param name="masterStorageName">The name of the master storage (cannot be null or empty)</param>
    /// <param name="operation">The type of transfer operation being performed</param>
    /// <exception cref="ArgumentException">Thrown when masterStorageName is null or empty</exception>
    public StorageOperationState(string masterStorageName, SmartTransferOperation operation)
    {
        if (string.IsNullOrEmpty(masterStorageName))
        {
            throw new ArgumentException("Master storage name cannot be null or empty", nameof(masterStorageName));
        }

        MasterStorageName = masterStorageName;
        Operation = operation;
    }

    /// <summary>
    /// Gets the number of distinct storages affected during this operation.
    /// </summary>
    public int StorageCount => _affectedStorages.Count;

    /// <summary>
    /// Returns a friendlier name for the affected storage(s).
    /// Currently just either the singular storage name, or otherwise the count, but in future
    /// maybe a concatenated version if there are 3 or less?
    /// </summary>
    public string GetStoragesDescription()
    {
        var storageCount = StorageCount;
        var result = storageCount.ToString();

        if (storageCount == 1)
        {
            var storage = _affectedStorages.SingleOrDefault();
            if (storage != null)
            {
                result = storage.GetName();
            }
        }

        return result;
    }

    /// <summary>
    /// Returns a friendlier name for the affected storage(s).
    /// Currently just either the singular storage name, or otherwise the count, but in future
    /// maybe a concatenated version if there are 3 or less?
    /// </summary>
    public string GetStacksDescription()
    {
        var stackCount = StackCount;
        var result = stackCount.ToString();

        if (ItemTypeCount == 1)
        {
            var itemType = _uniqueItems.FirstOrDefault();
            if (itemType != UniqueItemTypes.EMPTY)
            {
                var localisedName = ItemX.NameLocalisedOf(itemType);
                if (!string.IsNullOrEmpty(localisedName) && !string.IsNullOrWhiteSpace(localisedName))
                {
                    result = localisedName;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the number of distinct item stacks affected.
    /// </summary>
    public int StackCount => _affectedStacks.Count;

    /// <summary>
    /// Gets the number of unique item types moved.
    /// </summary>
    public int ItemTypeCount => _uniqueItems.Count;

    /// <summary>
    /// Gets the total number of items moved.
    /// </summary>
    public int ItemCount { get; set; } = 0;

    private bool ShouldRegisterStack(int initialStackSize, int currentStackSize, int maxStackSize)
    {
        return Operation switch
        {
            /* | Stack Before | Stack After | */

            // | Full         | Partial     |  — stack now has room to receive more
            // | Full         | Empty       |  — stack fully drained
            // | Partial      | Empty       |  — stack ran out
            SmartTransferOperation.Push => IsNotablePushState(initialStackSize, currentStackSize, maxStackSize),

            /* | Stack Before | Stack After | */

            // | Partial      | Full        |  — stack filled up
            // | Partial      | Partial     |  — stack grew but isn't full yet
            SmartTransferOperation.TopUp => IsNotableTopUpState(initialStackSize, currentStackSize),

            _ => false,
        };
    }

    // Push: stack transitioned to a notable "emptier" state — either it now has room, or it ran out.
    private static bool IsNotablePushState(int initial, int current, int max)
        => (initial == max && current < max)   // Full -> not full
        || (initial > 0 && current == 0);      // Non-empty -> empty

    // TopUp: a non-empty stack grew.
    private static bool IsNotableTopUpState(int initial, int current)
        => initial > 0 && current > initial;

    /// <summary>
    /// Records that items were affected by the operation
    /// </summary>
    internal void RecordTransfer(StorageTargetAdapter storage, ItemStack stack, int originalItemType, int initialStackSize, int currentStackSize, int maxStackSize, int transferCount)
    {
        if (storage == null || stack == null || maxStackSize <= 0 || transferCount <= 0)
        {
            return;
        }

        var shouldRegisterStack = ShouldRegisterStack(initialStackSize, currentStackSize, maxStackSize);
        if (shouldRegisterStack)
        {
            _ = _affectedStacks.Add(stack);
        }

        if (originalItemType != UniqueItemTypes.EMPTY)
        {
            _ = _uniqueItems.Add(originalItemType);
        }

        if (transferCount > 0)
        {
            _ = _affectedStorages.Add(storage);
            ItemCount += transferCount;
        }
    }

    internal void Reset()
    {
        _affectedStorages.Clear();
        _affectedStacks.Clear();
        _uniqueItems.Clear();

        ItemCount = 0;
    }

    public override string ToString()
    {
        return $"Storage operation on '{MasterStorageName}' affected {StackCount} stack(s) across {StorageCount} storage(s), having {ItemTypeCount} item type(s) and {ItemCount} item(s)";
    }
}