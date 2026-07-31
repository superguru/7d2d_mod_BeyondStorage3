using System;
using System.Collections.Generic;
using BeyondStorage.Data;
using BeyondStorage.Infrastructure;
using BeyondStorage.Storage.TransferTargets;
using BeyondStorage.UI;

namespace BeyondStorage.Storage.SmartSorting;

/// <summary>
/// Engine layer for smart push and pull transfers.
/// Owns the transfer loops, per-operation slot-map reclassification,
/// and the deferred MarkModified pattern.
/// Called exclusively by <see cref="SmartPushOperations"/>.
/// </summary>
internal static class ItemTransferEngine
{
    internal static readonly object s_smartPullLock = new();
    internal static readonly object s_smartPushLock = new();

    private static IReadOnlyList<StorageTargetAdapter> GetAdapterStorages(
        string methodName,
        StorageContext context,
        IReadOnlyList<ITransferAdapter> adapters)
    {
        if (adapters == null || adapters.Count == 0)
        {
            ModLogger.DebugLog($"{methodName}: was given null or no adapters, returning");
            return [];
        }

        var result = StorageTargetAdapter.CreateTargetAdapterList();

        for (int i = 0; i < adapters.Count; i++)
        {
            var adapter = adapters[i];
            if (adapter == null)
            {
                ModLogger.DebugLog($"{methodName}: found that adapter {i} was null, skipping");
                continue;
            }

            var targets = adapter.GetAdapters(context);
            if (targets == null || targets.Count == 0)
            {
#if DEBUG
                ModLogger.DebugLog($"{methodName}: received null or no targets from set '{adapter.GetAdapterName()}', skipping");
#endif
                continue;
            }

            result.AddRange(targets);
#if DEBUG
            ModLogger.DebugLog($"{methodName}: added {targets.Count} targets from '{adapter.GetAdapterName()}'");
#endif
        }

        return result;
    }

    private static (bool isRelevantSlotValid, (int maxStackSize, int itemType) value) IsTransferRelevantSlotValid(
    string methodName,
    StorageOperationState state,
    int slotIndex,
    ItemStack slot)
    {
#if DEBUG
        //ModLogger.DebugLog($"{methodName}: slot {slotIndex} in {state.MasterStorageName} is item {slot}");
#endif

        // 1. Not valid if empty
        if (ItemX.IsEmpty(slot))
        {
            return (isRelevantSlotValid: false, value: default);
        }

        // 2. Not valid if quest item
        var isQuestItem = ItemX.IsQuestItem(slot);
        if (isQuestItem)
        {
#if DEBUG
            ModLogger.DebugLog($"{methodName}: slot {slotIndex} in {state.MasterStorageName} is a quest item, skipping");
#endif
            return (isRelevantSlotValid: false, value: default);
        }

        // 3. Not valid if invalid max stack size
        int maxStackSize = ItemX.MaxStackSizeOf(slot);
        if (maxStackSize <= 0)
        {
#if DEBUG
            ModLogger.DebugLog($"{methodName}: slot {slotIndex} in {state.MasterStorageName} has invalid max stack size {maxStackSize}, skipping");
#endif
            return (isRelevantSlotValid: false, value: default);
        }

        // 4. Not valid if item is of invalid type
        int itemType = ItemX.ItemTypeOf(slot);
        if (itemType <= UniqueItemTypes.EMPTY)
        {
#if DEBUG
            ModLogger.DebugLog($"{methodName}: slot {slotIndex} in {state.MasterStorageName} is of invalid type {itemType}, skipping");
#endif
            return (isRelevantSlotValid: false, value: default);
        }

        return (isRelevantSlotValid: true, value: default);
    }

    internal static void PerformSmartLoadoutPull<T>(
        string methodName,
        StorageContext context,
        StorageSourceAdapter<T> loadout,
        IReadOnlyList<ITransferAdapter> sourceAdapters) where T : class
    {
        lock (s_smartPullLock)
        {
#if DEBUG
            //ModLogger.DebugLog($"{methodName}: Starting");
#endif
            if (loadout == null)
            {
                ModLogger.DebugLog($"{methodName}: Loadout is null, returning");
                return;
            }

            var sources = GetAdapterStorages(methodName, context, sourceAdapters);

            if (sources == null || sources.Count == 0)
            {
#if DEBUG
                ModLogger.DebugLog($"{methodName}: No source storages found, returning");
#endif
                return;
            }

#if DEBUG
            ModLogger.DebugLog($"{methodName}: Found {sources.Count} source storages, proceeding");
#endif
            var state = new StorageOperationState(loadout.GetName(), SmartTransferOperation.TopUp);

            PullSourceItemsToLoadout(methodName, state, loadout, sources);

            ModLogger.DebugLog($"{methodName}: {state}");

            if (state.StackCount > 0)
            {
                context.ShowLocalPlayerNotification(
                    SmartPullOperations.MSG_SMART_PULL_LOADOUT_RESULT,
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
        IReadOnlyList<ITransferAdapter> targetAdapters) where S : class
    {
        lock (s_smartPushLock)
        {
#if DEBUG
            //ModLogger.DebugLog($"{methodName}: Starting");
#endif
            if (source == null)
            {
                ModLogger.DebugLog($"{methodName}: Source is null, returning");
                return false;
            }

            var targets = GetAdapterStorages(methodName, context, targetAdapters);

            if (targets == null || targets.Count == 0)
            {
#if DEBUG
                ModLogger.DebugLog($"{methodName}: No target storages found, returning");
#endif
                return false;
            }

#if DEBUG
            ModLogger.DebugLog($"{methodName}: Found {targets.Count} target storages, proceeding");
#endif
            var state = new StorageOperationState(source.GetName(), SmartTransferOperation.Push);

            PushSourceItemsToTarget(methodName, state, source, targets, allowPushToEmpty: false);
            PushSourceItemsToTarget(methodName, state, source, targets, allowPushToEmpty: true);

            ModLogger.DebugLog($"{methodName}: {state}");

            var anyPushed = state.StackCount > 0;
            if (anyPushed)
            {
                context.ShowLocalPlayerNotification(
                    SmartPushOperations.MSG_SMART_PUSH_RESULT,
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
        StorageSourceAdapter<T> loadout,
        IReadOnlyList<StorageTargetAdapter> sources) where T : class
    {
        var loadoutSlotData = loadout.GetSlotData();
        var loadoutSlots = ItemX.GetFilteredItems(
            loadoutSlotData.AllSlots, StorageFilter.LockedOnly, loadoutSlotData.LockedSlots);
        var modifiedSources = new HashSet<StorageTargetAdapter>();

        for (int i = 0; i < loadoutSlots.Length; i++)
        {
            var loadoutSlot = loadoutSlots[i];

            (bool isRelevantSlotValid, (int maxStackSize, int itemType)) = IsTransferRelevantSlotValid(methodName, state, i, loadoutSlot);
            if (!isRelevantSlotValid)
            {
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
                if (source.IsSameSource(loadout))
                {
                    continue;
                }

                if (PullToLoadoutSlots(state, loadoutSlot, source, itemType, maxStackSize, ref loadoutSlotRequiredAmount))
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

            (bool isRelevantSlotValid, (int maxStackSize, int itemType)) = IsTransferRelevantSlotValid(methodName, state, i, sourceSlot);
            if (!isRelevantSlotValid)
            {
                continue;
            }

            int sourceSlotRemaining = ItemX.CurrentStackSizeOf(sourceSlot);

            for (int k = 0; k < targets.Count; k++)
            {
                if (sourceSlotRemaining <= 0)
                {
                    break;
                }

                var target = targets[k];
                if (target.IsSameSource(source))
                {
#if DEBUG
                    ModLogger.DebugLog($"{methodName}: target {target.GetName()} is the same as source {source.GetName()}");
#endif
                    continue;
                }

                PushToTarget(state, source, sourceSlot, target, itemType, allowPushToEmpty, maxStackSize, ref sourceSlotRemaining);
            }
        }
    }

    private static bool PullToLoadoutSlots(
        StorageOperationState state,
        ItemStack loadoutSlot,
        StorageTargetAdapter source,
        int itemType,
        int maxStackSize,
        ref int loadoutSlotRequiredAmount)
    {
        var originalItemType = ItemX.ItemTypeOf(loadoutSlot);
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
            state.RecordTransfer(source, loadoutSlot, originalItemType, initialStackSize, currentStackSize, maxStackSize, transferCount);
            return true;
        }

        return false;
    }

    private static void PushToTarget<S>(
        StorageOperationState state,
        StorageSourceAdapter<S> source,
        ItemStack sourceSlot,
        StorageTargetAdapter target,
        int itemType,
        bool allowPushToEmpty,
        int maxStackSize,
        ref int sourceSlotRemaining) where S : class
    {
        var originalItemType = ItemX.ItemTypeOf(sourceSlot);
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

            state.RecordTransfer(target, sourceSlot, originalItemType, initialStackSize, currentStackSize, maxStackSize, transferCount);
        }
    }
}
