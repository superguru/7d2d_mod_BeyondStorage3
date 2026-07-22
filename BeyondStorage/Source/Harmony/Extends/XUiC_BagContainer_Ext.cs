using System.Linq;
using BeyondStorage.Data;
using BeyondStorage.Game.UI;
using BeyondStorage.Infrastructure;
using BeyondStorage.UI;
using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

///
/// Introduced in v3.0.0 of 7 Days to Die, where it was only used for vehicles
/// 

[HarmonyPatch(typeof(XUiC_BagContainer))]
internal static class XUiC_BagContainer_Ext
{
    // Store the previous LockedSlots state for comparison
    private static PackedBoolArray s_previousLockedSlots = null;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_BagContainer.Init))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_BagContainer_Init_Postfix(XUiC_BagContainer __instance)
    {
#if DEBUG
        //sconst string d_MethodName = nameof(XUiC_BagContainer_Init_Postfix);
#endif
        var btnBeyondSmartDronePullLoadout = UIControlHelpers.GetSmartDroneInventoryPullLoadoutButton(__instance);
        if (btnBeyondSmartDronePullLoadout != null)
        {
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: Smart drone pull loadout button initialized");
#endif
            btnBeyondSmartDronePullLoadout.OnPress += SmartSortingCommon.SmartDroneInventoryPullLoadout_EventHandler;
        }

        var btnBeyondSmartPushButton = UIControlHelpers.GetSmartVehiclePushButton(__instance);
        if (btnBeyondSmartPushButton != null)
        {
            btnBeyondSmartPushButton.OnPress += SmartSortingCommon.SmartVehiclePush_EventHandler;
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: Smart vehicle push button initialized");
#endif
        }

        var btnBeyondSmartDroppedLootPushButton = UIControlHelpers.GetSmartDroppedLootWindowPushButton(__instance);
        if (btnBeyondSmartDroppedLootPushButton != null)
        {
            btnBeyondSmartDroppedLootPushButton.OnPress += SmartSortingCommon.SmartDroppedLootPush_EventHandler;
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: Smart dropped loot push button initialized");
#endif
        }

        var btnBeyondSmartPullButton = UIControlHelpers.GetSmartVehiclePullLoadoutButton(__instance);
        if (btnBeyondSmartPullButton != null)
        {
            btnBeyondSmartPullButton.OnPress += SmartSortingCommon.SmartVehiclePullLoadout_EventHandler;
#if DEBUG
            //ModLogger.DebugLog($"{d_MethodName}: Smart vehicle pull loadout button initialized");
#endif
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(XUiC_BagContainer.GetBindingValueInternal))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static bool XUiC_BagContainer_GetBindingValueInternal_Prefix(XUiC_BagContainer __instance, ref string _value, string _bindingName, ref bool __result)
    {
        switch (_bindingName)
        {
            case "bs_is_drone_window_open":
                _value = WindowStateManager.IsDroneWindowOpen() ? "true" : "false";
                __result = true;
                return false; // Skip original method

            case "bs_is_vehicle_window_open":
                _value = WindowStateManager.IsVehicleWindowOpen() ? "true" : "false";
                __result = true;
                return false; // Skip original method
        }

        return true; // Run original method for other bindings
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(XUiC_BagContainer.UpdateLockedSlots))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_BagContainer_UpdateLockedSlots_Prefix(XUiC_BagContainer __instance, XUiC_ContainerStandardControls _csc)
    {
#if DEBUG
        const string d_MethodName = nameof(XUiC_BagContainer_UpdateLockedSlots_Prefix);
#endif
        if (_csc == null)
        {
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: _csc parameter is null");
#endif
            s_previousLockedSlots = null;
            return;
        }

        // Save the current LockedSlots state before the update
        s_previousLockedSlots = _csc.LockedSlots;

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Saved LockedSlots state: {(s_previousLockedSlots != null ? $"Count={s_previousLockedSlots.Length}" : "null")}");
#endif
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_BagContainer.OnClose))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_BagContainer_OnClose(XUiC_BagContainer __instance)
    {
#if DEBUG
        //const string d_MethodName = nameof(XUiC_BagContainer_OnClose);
#endif

        WindowStateManager.OnBagContainerClosing(__instance);

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Bag Storage Window Closed");
#endif
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_BagContainer.OnOpen))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_BagContainer_OnOpen(XUiC_BagContainer __instance)
    {
#if DEBUG
        //const string d_MethodName = nameof(XUiC_BagContainer_OnOpen);
#endif

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName} Start");
#endif

        WindowStateManager.OnBagContainerOpening(__instance);

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Refreshing bindings");
#endif
        __instance?.RefreshBindings();
#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Bindings refreshed");
#endif

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName} End. Bag container opened for {__instance.containerName}");
#endif
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(XUiC_BagContainer.UpdateLockedSlots))]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void XUiC_VehicleContainer_UpdateLockedSlots_Postfix(XUiC_BagContainer __instance, XUiC_ContainerStandardControls _csc)
    {
        if (_csc != null)
        {
            var currentLockedSlots = _csc.LockedSlots;
            if (currentLockedSlots == null)
            {
                return;
            }

            ItemStack itemStack = null;

            // For vehicle containers, we need to check the vehicle's bag items for currency
            var vehicleBag = __instance?.Bag ?? null;
            if (vehicleBag?.items != null)
            {
                // Check if any of the vehicle bag items contain currency
                bool containsCurrency = vehicleBag.items.Any(item => CurrencyCache.IsCurrencyItem(item));
                if (containsCurrency)
                {
                    // Trigger a currency refresh after slot lock changes when currency is present
                    itemStack = CurrencyCache.GetEmptyCurrencyStack();
                }
            }

            UIRefreshHelper.LogAndRefreshUI(StackOps.Stack_LockStateChange_Operation, itemStack: itemStack);
        }
    }

#if DEBUG
    /// <summary>
    /// Compares two PackedBoolArray instances for equality.
    /// Returns true if both are null, or if both have the same content.
    /// </summary>
    private static bool AreLockedSlotsEqual(PackedBoolArray previous, PackedBoolArray current)
    {
        // Both null - equal
        if (previous == null && current == null)
        {
            return true;
        }

        // One null, one not - not equal
        if (previous == null || current == null)
        {
            return false;
        }

        // Different lengths - not equal
        if (previous.Length != current.Length)
        {
            return false;
        }

        // Compare each element
        for (int i = 0; i < previous.Length; i++)
        {
            if (previous[i] != current[i])
            {
                return false;
            }
        }

        return true;
    }
#endif
}
