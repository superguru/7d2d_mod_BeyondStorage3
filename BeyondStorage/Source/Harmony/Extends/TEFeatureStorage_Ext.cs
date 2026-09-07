extern alias DotNetSystem;

using System;
using BeyondStorage.Entities;
using HarmonyLib;

#if DEBUG
using BeyondStorage.Infrastructure;
#endif

namespace BeyondStorage.Harmony.Extends;

[HarmonyPatch(typeof(TEFeatureStorage))]
#if DEBUG
[HarmonyDebug]
#endif
internal static class TEFeatureStorage_Ext
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TEFeatureStorage.InitBlockActivationCommands), [typeof(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData>)])]
#if DEBUG
    [HarmonyDebug]
#endif
    private static bool TEFeatureStorage_InitBlockActivationCommands_Prefix(TEFeatureStorage __instance)
    {
        bool shouldRun = __instance?.FeatureData != null;
#if DEBUG
        if (!shouldRun)
        {
            ModLogger.DebugLog($"{nameof(TEFeatureStorage_InitBlockActivationCommands_Prefix)}: Skipping — FeatureData is null on instance {__instance}");
        }
#endif
        return shouldRun;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TEFeatureStorage.InitBlockActivationCommands), [typeof(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData>)])]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void TEFeatureStorage_InitBlockActivationCommands_Postfix(TEFeatureStorage __instance, Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
    {
        // Icons seem swapped below, but it shows a nice green tick if the block is currently ✅On, and a red cross if it's currently ❌Off
        _addCallback(new BlockActivationCommand(_text: "DisableForInventoryNetwork", _icon: "enabled_for_inventory_network", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, __instance.FeatureData);
        _addCallback(new BlockActivationCommand(_text: "EnableForInventoryNetwork", _icon: "disabled_for_inventory_network", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, __instance.FeatureData);
        // See TEFeatureAbs patch methods for the AllowBlockActivationCommand extension
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TEFeatureStorage.OnBlockActivated), [typeof(ReadOnlySpan<char>), typeof(WorldBase), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayerLocal)])]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void TEFeatureStorage_OnBlockActivated_Postfix(TEFeatureStorage __instance, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player, ref bool __result)
    {
#if DEBUG
        const string d_MethodName = nameof(TEFeatureStorage_OnBlockActivated_Postfix);
#endif

        if (__result)
        {
            return; // original handled the command
        }

        bool isCmdTurnOff = __instance.CommandIs(_commandName, "DisableForInventoryNetwork");
        bool isCmdTurnOn = !isCmdTurnOff && __instance.CommandIs(_commandName, "EnableForInventoryNetwork");
        if (!(isCmdTurnOff || isCmdTurnOn))
        {
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: Cannot handle this command");
#endif
            return;
        }

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Going to handle command isTurnOff={isCmdTurnOff}, isTurnOn={isCmdTurnOn}");
#endif
        if (isCmdTurnOff)
        {
            BlockInventoryNetworkStates.TurnOffForInventoryNetwork(_blockPos);
        }
        else
        {
            BlockInventoryNetworkStates.TurnOnForInventoryNetwork(_blockPos);
        }

        __result = true; // we did handle this command
    }
}
