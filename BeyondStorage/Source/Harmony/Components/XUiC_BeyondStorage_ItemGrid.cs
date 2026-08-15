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
}