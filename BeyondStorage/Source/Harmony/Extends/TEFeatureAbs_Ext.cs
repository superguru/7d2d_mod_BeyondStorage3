
using System;
using BeyondStorage.Entities;
using BeyondStorage.Infrastructure;
using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

[HarmonyPatch(typeof(TEFeatureAbs))]
#if DEBUG
[HarmonyDebug]
#endif
internal static class TEFeatureAbs_Ext
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TEFeatureAbs.AllowBlockActivationCommand), [typeof(ITileEntityFeature), typeof(ReadOnlySpan<char>), typeof(WorldBase), typeof(Vector3i), typeof(BlockValue), typeof(EntityAlive)])]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void TEFeatureAbs_AllowBlockActivationCommand(TEFeatureSignable __instance, ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, ref bool __result)
    {
#if DEBUG
        const string d_MethodName = nameof(TEFeatureAbs.AllowBlockActivationCommand);
#endif
        if (__instance == null)
        {
            ModLogger.DebugLog($"{d_MethodName}: Cannot operate on null block");

            __result = false;  // we don't handle this command
            return;
        }

        var allowConsumeOps = ConsumeCapabilityCheckList.AnyPasses(__instance);
        if (!allowConsumeOps)
        {
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: cmd=?, __result={__result}, no checks passed");
#endif
            return;  // we won't check anything else if this thing isn't even eligible for consumption
        }

        if (__instance.CommandIs(_commandName, "Consume_Off"))
        {
            __result = BlockConsumeStates.IsConsumeOn(_blockPos);
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: cmd=consume_off, __result={__result}");
#endif
            return;
        }

        if (__instance.CommandIs(_commandName, "Consume_On"))
        {
            __result = BlockConsumeStates.IsConsumeOff(_blockPos);
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: cmd=consume_on, __result={__result}");
#endif
            return;
        }
    }
}
