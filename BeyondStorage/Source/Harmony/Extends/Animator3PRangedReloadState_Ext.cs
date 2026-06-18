using BeyondStorage.Game.Ranged;
using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

[HarmonyPatch(typeof(Animator3PRangedReloadState))]
internal static class Animator3PRangedReloadState_Ext
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Animator3PRangedReloadState.GetAmmoCount))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void Animator3PRangedReloadState_GetAmmoCount_Postfix(ref int __result, ItemValue ammo, int modifiedMagazineSize)
    {
        __result = AnimatorCommon.GetAmmoCount(ammo, __result, modifiedMagazineSize);
    }
}