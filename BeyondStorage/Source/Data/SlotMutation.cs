using System;

namespace BeyondStorage.Data;

/// <summary>
/// Canonical primitives for mutating a single live <see cref="ItemStack"/> slot.
/// Both the consume path (<see cref="BeyondStorage.Storage.StorageItemRemovalService"/>) and the
/// smart-transfer path (<see cref="BeyondStorage.Storage.ItemTransferEngine"/>) call these methods
/// so that the exact mutation semantics live in exactly one place.
/// </summary>
internal static class SlotMutation
{
    /// <summary>
    /// Drains up to <paramref name="countToRemove"/> items from <paramref name="stack"/>.
    /// For stackable items the count is decremented; the slot is cleared when it reaches zero.
    /// For non-stackable items the slot is always cleared and exactly 1 is returned.
    /// </summary>
    /// <returns>The number of items actually removed (0 when the slot was already empty).</returns>
    internal static int Drain(ItemStack stack, int countToRemove, bool canStack)
    {
        if (stack == null || stack.count <= 0 || countToRemove <= 0)
        {
            return 0;
        }

        if (canStack)
        {
            int actual = Math.Min(stack.count, countToRemove);
            stack.count -= actual;
            if (stack.count == 0)
            {
                stack.Clear();
            }
            return actual;
        }
        else
        {
            stack.Clear();
            return 1;
        }
    }

    /// <summary>
    /// Transfers up to <paramref name="transferLimit"/> items from <paramref name="sourceSlot"/>
    /// into <paramref name="targetSlot"/>, bounded by the remaining capacity of the target.
    /// If the target slot is empty its <see cref="ItemStack.itemValue"/> is initialised from
    /// the source before the count is applied.
    /// </summary>
    /// <remarks>
    /// Does NOT modify the source slot. Callers own source-side bookkeeping because the pull
    /// path must call ReclassifySlot BETWEEN deducting the count and calling Clear() —
    /// that ordering is a deliberate bug-fix and must not be encapsulated away.
    /// </remarks>
    /// <returns>The number of items actually transferred into the target slot.</returns>
    internal static int Fill(ItemStack sourceSlot, ItemStack targetSlot, int maxStackSize, int transferLimit)
    {
        if (sourceSlot == null || targetSlot == null)
        {
            return 0;
        }

        int targetSlotSpace = maxStackSize - ItemX.CurrentStackSizeOf(targetSlot);
        int transferAmount = Math.Min(transferLimit, targetSlotSpace);
        if (transferAmount <= 0)
        {
            return 0;
        }

        int targetCountBefore = targetSlot.count;

        if (ItemX.ItemTypeOf(targetSlot) == UniqueItemTypes.EMPTY || targetSlot.count <= 0)
        {
            targetSlot.itemValue = sourceSlot.itemValue.Clone();
            targetSlot.count = 0;
            targetCountBefore = 0;
        }

        targetSlot.count += transferAmount;

        return targetSlot.count - targetCountBefore;
    }
}
