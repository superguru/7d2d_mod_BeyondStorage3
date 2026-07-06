using System;
using System.Collections.Generic;
using BeyondStorage.Data;
using BeyondStorage.Infrastructure;
using BeyondStorage.UI;

namespace BeyondStorage.Storage;

/// <summary>
/// Engine layer for smart push and pull transfers.
/// Owns the transfer loops, per-operation slot-map reclassification,
/// and the deferred MarkModified pattern.
/// Called exclusively by <see cref="SmartSortingFunctions"/>.
/// </summary>
internal static class ItemTransferEngine
{
    internal static readonly object s_smartPullLock = new();
    internal static readonly object s_smartPushLock = new();

    internal static void PerformSmartLoadoutPull<T>(
        string methodName,
        StorageContext context,
        StorageSourceAdapter<T> loadout,
        IReadOnlyList<StorageTargetAdapter> sources) where T : class
    {
        lock (s_smartPullLock)
        {
            if (loadout == null)
            {
                ModLogger.DebugLog($"{methodName}: Loadout is null, returning");
                return;
            }

            if (sources == null || sources.Count == 0)
            {
                ModLogger.DebugLog($"{methodName}: No source storages found, returning");
                return;
            }

            var state = new StorageOperationState(loadout.GetName(), SmartTransferOperation.TopUp);

            PullSourceItemsToLoadout(methodName, state, sources, loadout);

            ModLogger.DebugLog($"{methodName}: {state}");

            if (state.StackCount > 0)
            {
                context.ShowLocalPlayerNotification(
                    SmartSortingFunctions.MSG_SMART_PULL_LOADOUT_RESULT,
                    state.StackCount,
                    state.MasterStorageName);
                context.InvalidateCache();
            }

            UIRefreshHelper.ValidateAndRefreshUI(context, methodName);
        }
    }

    internal static bool PerformSmartPush<S>(
        string methodName,
        StorageContext context,
        StorageSourceAdapter<S> source,
        IReadOnlyList<StorageTargetAdapter> targets,
        Func<StorageContext, IReadOnlyList<StorageTargetAdapter>> onMissionFallback) where S : class
    {
        lock (s_smartPushLock)
        {
            if (source == null)
            {
                ModLogger.DebugLog($"{methodName}: Source is null, returning");
                return false;
            }

            if (targets == null || targets.Count == 0)
            {
                ModLogger.DebugLog($"{methodName}: No target storages found, trying to find if On Mission");

                targets = onMissionFallback(context);

                if (targets == null || targets.Count == 0)
                {
                    ModLogger.DebugLog($"{methodName}: No on mission target storages found, returning");
                    return false;
                }

                ModLogger.DebugLog($"{methodName}: Found {targets.Count} on mission target storages, proceeding with smart push");
            }

            var state = new StorageOperationState(source.GetName(), SmartTransferOperation.Push);

            PushSourceItemsToTarget(methodName, state, source, targets, allowPushToEmpty: false);
            PushSourceItemsToTarget(methodName, state, source, targets, allowPushToEmpty: true);

            ModLogger.DebugLog($"{methodName}: {state}");

            var anyPushed = state.StackCount > 0;
            if (anyPushed)
            {
                context.ShowLocalPlayerNotification(
                    SmartSortingFunctions.MSG_SMART_PUSH_RESULT,
                    state.StackCount,
                    state.MasterStorageName,
                    state.StorageCount);
                context.InvalidateCache();
            }

            UIRefreshHelper.ValidateAndRefreshUI(context, methodName);
            return anyPushed;
        }
    }

    private static void PullSourceItemsToLoadout<T>(
        string methodName,
        StorageOperationState state,
        IReadOnlyList<StorageTargetAdapter> sources,
        StorageSourceAdapter<T> loadout) where T : class
    {
        var loadoutSlotData = loadout.GetSlotData();
        var loadoutSlots = ItemX.GetFilteredItems(
            loadoutSlotData.AllSlots, StorageFilter.LockedOnly, loadoutSlotData.LockedSlots);
        var modifiedSources = new HashSet<StorageTargetAdapter>();

        for (int i = 0; i < loadoutSlots.Length; i++)
        {
            var loadoutSlot = loadoutSlots[i];
            if (ItemX.IsEmpty(loadoutSlot))
            {
                ModLogger.DebugLog($"{methodName}: Loadout slot {i} is empty, skipping");
                continue;
            }

            int maxStackSize = ItemX.MaxStackSizeOf(loadoutSlot);
            if (maxStackSize <= 0)
            {
#if DEBUG
                ModLogger.DebugLog($"{methodName}: Loadout slot {i} in {state.MasterStorageName} has invalid max stack size {maxStackSize}, skipping");
#endif
                continue;
            }

            int itemType = ItemX.ItemTypeOf(loadoutSlot);
            if (itemType == UniqueItemTypes.EMPTY)
            {
                ModLogger.DebugLog($"{methodName}: Loadout slot {i} has invalid item type {itemType}, skipping");
                continue;
            }

            int loadoutSlotRequiredAmount = maxStackSize - ItemX.CurrentStackSizeOf(loadoutSlot);

            for (int k = 0; k < sources.Count; k++)
            {
                if (loadoutSlotRequiredAmount <= 0)
                {
                    break;
                }

                var source = sources[k];
                if (source.HasSameSource(loadout))
                {
                    continue;
                }

                if (PullToLoadoutSlots(methodName, state, loadoutSlot, source, itemType, maxStackSize, ref loadoutSlotRequiredAmount))
                {
                    modifiedSources.Add(source);
                }
            }
        }

        // Defer MarkModified until after all iterations to prevent game bag rebuilds
        // from invalidating loadoutSlot references mid-loop.
        foreach (var modifiedSource in modifiedSources)
        {
            modifiedSource.MarkModified();
        }

        if (modifiedSources.Count > 0)
        {
            loadout.MarkModified();
        }
    }

    private static void PushSourceItemsToTarget<S>(
        string methodName,
        StorageOperationState state,
        StorageSourceAdapter<S> source,
        IReadOnlyList<StorageTargetAdapter> targets,
        bool allowPushToEmpty) where S : class
    {
        // Re-read each pass so slots emptied in the partial-fill pass are naturally
        // excluded from the empty-fill pass without extra filtering.
        var sourceSlotData = source.GetSlotData();
        var sourceSlots = ItemX.GetFilteredItems(
            sourceSlotData.AllSlots, StorageFilter.UnlockedOnly, sourceSlotData.LockedSlots);

        for (int i = 0; i < sourceSlots.Length; i++)
        {
            var sourceSlot = sourceSlots[i];
            if (ItemX.IsEmpty(sourceSlot))
            {
                continue;
            }

            int maxStackSize = ItemX.MaxStackSizeOf(sourceSlot);
            if (maxStackSize <= 0)
            {
#if DEBUG
                ModLogger.DebugLog($"{methodName}: Source slot {i} in {state.MasterStorageName} has invalid max stack size {maxStackSize}, skipping");
#endif
                continue;
            }

            int itemType = ItemX.ItemTypeOf(sourceSlot);
            int sourceSlotRemaining = ItemX.CurrentStackSizeOf(sourceSlot);

            for (int k = 0; k < targets.Count; k++)
            {
                if (sourceSlotRemaining <= 0)
                {
                    break;
                }

                var target = targets[k];
                if (target.HasSameSource(source))
                {
                    continue;
                }

                PushToTarget(methodName, state, source, sourceSlot, target, itemType, allowPushToEmpty, maxStackSize, ref sourceSlotRemaining);
            }
        }
    }

    private static bool PullToLoadoutSlots(
        string methodName,
        StorageOperationState state,
        ItemStack loadoutSlot,
        StorageTargetAdapter source,
        int itemType,
        int maxStackSize,
        ref int loadoutSlotRequiredAmount)
    {
        int transferCount = 0;
        int initialStackSize = maxStackSize - loadoutSlotRequiredAmount;

        while (loadoutSlotRequiredAmount > 0)
        {
            var sourceSlot = source.GetNextPopulatedStackFor(itemType);
            if (sourceSlot == null)
            {
                break;
            }

            int sourceSlotActualCount = ItemX.CurrentStackSizeOf(sourceSlot);

            int cappedTransferLimit = Math.Min(sourceSlotActualCount, loadoutSlotRequiredAmount);
            if (cappedTransferLimit <= 0)
            {
                // Source slot is depleted despite being in the populated map — avoid infinite loop
                break;
            }

            // SlotMutation.Fill writes to loadoutSlot only. Source-side bookkeeping stays
            // here to preserve the critical ordering: deduct → ReclassifySlot → Clear.
            var transferAmount = SlotMutation.Fill(sourceSlot, loadoutSlot, maxStackSize, cappedTransferLimit);

            sourceSlot.count = sourceSlotActualCount - transferAmount;
            loadoutSlotRequiredAmount -= transferAmount;
            transferCount += transferAmount;

            if (transferAmount > 0)
            {
                // CRITICAL: ReclassifySlot must be called while itemValue is still valid,
                // before Clear() removes it. This ordering was a deliberate bug-fix.
                source.ReclassifySlot(sourceSlot);
                if (sourceSlot.count == 0)
                {
                    sourceSlot.Clear();
                }
            }
            else
            {
                // No items transferred; source slot may be depleted — avoid infinite loop
                break;
            }
        }

        if (transferCount > 0)
        {
            int currentStackSize = maxStackSize - loadoutSlotRequiredAmount;
            state.RecordTransfer(source, loadoutSlot, initialStackSize, currentStackSize, maxStackSize, transferCount);
            return true;
        }

        return false;
    }

    private static void PushToTarget<S>(
        string methodName,
        StorageOperationState state,
        StorageSourceAdapter<S> source,
        ItemStack sourceSlot,
        StorageTargetAdapter target,
        int itemType,
        bool allowPushToEmpty,
        int maxStackSize,
        ref int sourceSlotRemaining) where S : class
    {
        int transferCount = 0;
        int initialStackSize = sourceSlotRemaining;

        while (sourceSlotRemaining > 0)
        {
            var targetSlot = target.GetNextPartialStackFor(itemType);

            if (targetSlot == null)
            {
                if (!allowPushToEmpty)
                {
                    break;
                }

                targetSlot = target.GetNextEmptyStackFor(itemType);
                if (targetSlot == null)
                {
                    break;
                }
            }

            // SlotMutation.Fill writes to targetSlot. Push source is player inventory
            // (not a StorageTargetAdapter), so no ReclassifySlot is needed on the source.
            var transferAmount = SlotMutation.Fill(sourceSlot, targetSlot, maxStackSize, sourceSlotRemaining);

            if (transferAmount > 0)
            {
                sourceSlotRemaining -= transferAmount;
                sourceSlot.count = sourceSlotRemaining;
                if (sourceSlotRemaining == 0)
                {
                    sourceSlot.Clear();
                }
                transferCount += transferAmount;
                target.ReclassifySlot(targetSlot);
            }
            else
            {
                // No items transferred; slot may already be full — avoid infinite loop
                break;
            }
        }

        if (transferCount > 0)
        {
            int currentStackSize = sourceSlotRemaining;
            source.MarkModified();
            target.MarkModified();
            state.RecordTransfer(target, sourceSlot, initialStackSize, currentStackSize, maxStackSize, transferCount);
        }
    }
}
