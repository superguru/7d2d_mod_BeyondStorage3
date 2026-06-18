using BeyondStorage.Game.Ranged;
using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

[HarmonyPatch(typeof(AnimatorRangedReloadState))]
internal static class AnimatorRangedReloadState_Ext
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(AnimatorRangedReloadState.GetAmmoCount))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void AnimatorRangedReloadState_GetAmmoCount_Postfix(ref int __result, ItemValue ammo, int modifiedMagazineSize)
    {
        __result = AnimatorCommon.GetAmmoCount(ammo, __result, modifiedMagazineSize);
    }
}