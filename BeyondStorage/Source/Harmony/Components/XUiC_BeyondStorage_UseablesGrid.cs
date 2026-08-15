using UnityEngine.Scripting;

namespace BeyondStorage.Harmony.Components;

[Preserve]
public class XUiC_BeyondStorage_UseablesGrid : XUiC_BeyondStorage_ItemGrid
{
    public override void OnOpen()
    {
        base.OnOpen();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public override void UpdateBackend(ItemStack[] stackList)
    {
        base.UpdateBackend(stackList);  // TODO: Should we be doing this?
        windowGroup.Controller.SetAllChildrenDirty();
    }
}
