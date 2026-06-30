
using System;
using BeyondStorage.Entities;
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
    private static void TEFeatureAbs_AllowBlockActivationCommand(TEFeatureAbs __instance, ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, ref bool __result)
    {
        if (__instance is TEFeatureStorage storage)
        {
            var isPlayerStorage = storage.bPlayerStorage;

            if (__instance.CommandIs(_commandName, "Consume_Off"))
            {
                __result = isPlayerStorage && BlockConsumeStates.IsConsumeOn(_blockPos);
                return;
            }

            if (__instance.CommandIs(_commandName, "Consume_On"))
            {
                __result = isPlayerStorage && BlockConsumeStates.IsConsumeOff(_blockPos);
                return;
            }
        }

        __result = true;
    }
}
