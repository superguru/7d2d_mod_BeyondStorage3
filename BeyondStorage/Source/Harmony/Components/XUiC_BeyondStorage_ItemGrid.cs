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
    public virtual void LockCells()
    {
        var controllers = GetItemStackControllers();
        for (int i = 0; i < controllers.Length; i++)
        {
            var controller = controllers[i];
            controller.IsLocked = true;
            controller.lockType = XUiC_ItemStack.LockTypes.Shell;
            //controller.lockSprite
        }
    }
}