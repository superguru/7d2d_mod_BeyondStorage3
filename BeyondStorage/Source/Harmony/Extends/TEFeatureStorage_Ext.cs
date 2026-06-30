using System;
using BeyondStorage.Entities;
using BeyondStorage.Infrastructure;
using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

[HarmonyPatch(typeof(TEFeatureStorage))]
#if DEBUG
[HarmonyDebug]
#endif
internal static class TEFeatureStorage_Ext
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TEFeatureStorage.InitBlockActivationCommands), [typeof(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData>)])]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void TEFeatureStorage_InitBlockActivationCommands_Postfix(TEFeatureSignable __instance, Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
    {
        //TODO: Lazy add TEFeatureAbs instance check for TEFeatureStorage.bPlayerStorage to delegate registry in a new class in Game\Features or Game\Components

        _addCallback(new BlockActivationCommand("Consume_Off", "consume_off", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, __instance?.FeatureData);
        _addCallback(new BlockActivationCommand("Consume_On", "consume_on", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, __instance?.FeatureData);
        // See TEFeatureAbs patch methods for the AllowBlockActivationCommand extension
    }

    private static void IsPlayerStorage(TEFeatureAbs __instance, ref bool __result)
    {

    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TEFeatureStorage.OnBlockActivated), [typeof(ReadOnlySpan<char>), typeof(WorldBase), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayerLocal)])]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void TEFeatureStorage_OnBlockActivated_Postfix(TEFeatureSignable __instance, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player, ref bool __result)
    {
#if DEBUG
        const string d_MethodName = nameof(TEFeatureStorage.OnBlockActivated);
#endif
        if (__result)
        {
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: Original method handled command");
#endif
            return; // original handled the command
        }

        if (__instance == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: Cannot operate on null block");

            __result = false;  // we don't handle this command
            return;
        }

        bool isCmdTurnConsumeOff = __instance.CommandIs(_commandName, "Consume_Off");
        bool isCmdTurnConsumeOn = !isCmdTurnConsumeOff && __instance.CommandIs(_commandName, "Consume_On");
        if (!(isCmdTurnConsumeOff || isCmdTurnConsumeOn))
        {
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: Cannot handle this command");
#endif
            __result = false;  // we did not handle this command
            return;
        }

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Going to handle command isTurnConsumeOff={isCmdTurnConsumeOff}, isTurnConsumeOn={isCmdTurnConsumeOn}");
#endif
        if (isCmdTurnConsumeOff)
        {
            BlockConsumeStates.TurnConsumeOff(_blockPos);
        }
        else
        {
            BlockConsumeStates.TurnConsumeOn(_blockPos);
        }

#if DEBUG
        ModLogger.DebugLog($"{d_MethodName}: Command handled and now ConsumeOn={BlockConsumeStates.IsConsumeOn(_blockPos)}");
#endif
    }
}
