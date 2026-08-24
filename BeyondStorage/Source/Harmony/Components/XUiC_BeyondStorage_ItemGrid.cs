using BeyondStorage.Data;
using UnityEngine.Scripting;

namespace BeyondStorage.Harmony.Components;

[Preserve]
public class XUiC_BeyondStorage_ItemGrid : XUiC_ItemStackGrid
{
    public override XUiC_ItemStack.StackLocationTypes StackLocation
    {
        [PublicizedFrom(EAccessModifier.Protected)]
        get
        {
            return XUiC_ItemStack.StackLocationTypes.Backpack;
        }
    }

    public override void OnOpen()
    {
        IsDirty = true;
    }

    public override void OnClose()
    {
        IsDirty = true;
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public override void UpdateBackend(ItemStack[] stackList)
    {
    }

    /// <summary>
    /// Cells here are synthetic display stacks aggregated across possibly many storage sources, not
    /// a reference to any single real slot, so picking one up would hand the player a free copy of
    /// a real item without ever removing it from storage — a duplication bug, not just a UX wrinkle.
    /// IsLocked alone gates the entire mouse/gamepad interaction block on XUiC_ItemStack (click,
    /// drag, swap, partial-stack pickup), so that's the only flag needed to prevent pickup.
    /// AllowDropping = false was tried too as a second layer, but it's what drew a "denied" cursor
    /// icon on hover, and it's redundant given IsLocked already blocks everything — removed.
    /// Re-applied after every refresh since SetStacks recreates the underlying ItemStack.
    /// </summary>
    internal virtual void LockCells()
    {
        var controllers = GetItemStackControllers();
        for (int i = 0; i < controllers.Length; i++)
        {
            var controller = controllers[i];
            controller.IsLocked = true;

            // Must be non-None: XUiC_ItemStack.updateLockTypeIcon() only skips resetting lockSprite
            // back to "" when IsLocked && lockType != LockTypes.None, so None would wipe out
            // whatever SetLockSprite assigns below on the very next dirty refresh.
            controller.lockType = XUiC_ItemStack.LockTypes.Shell;

            SetLockSprite(controller);
        }
    }

    /// <summary>
    /// Picks the lock icon to match what the item would actually do if used — the same icon names
    /// vanilla's own ItemActionEntryUse assigns to its action-list buttons per ConsumeType, so this
    /// reads consistently with the rest of the game's UI instead of inventing new iconography.
    /// </summary>
    internal virtual void SetLockSprite(XUiC_ItemStack controller)
    {
        var stack = controller?.ItemStack;
        if (stack == null || stack.IsEmpty())
        {
            return;
        }

        var itemType = stack.itemValue.type;

        if (UseableItemStore.IsHealItem(itemType))
        {
            controller.lockSprite = "ui_game_symbol_medical";
        }
        else if (UseableItemStore.IsDrinkItem(itemType))
        {
            controller.lockSprite = "ui_game_symbol_water";
        }
        else if (UseableItemStore.IsFoodItem(itemType))
        {
            controller.lockSprite = "ui_game_symbol_fork";
        }
        else
        {
            controller.lockSprite = "ui_game_symbol_check";
        }
    }
}