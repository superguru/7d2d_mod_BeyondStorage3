using UnityEngine.Scripting;

namespace BeyondStorage.Harmony.Components;

/// <summary>
/// Item stack cell used by the Useables grid. The shared item_stack template
/// (Config/XUi_InGame/templates.xml) has a "cancel" sprite — a red ui_game_symbol_x overlay —
/// bound to visible="{# ishovered and islocked}", which is vanilla's generic "this slot is locked"
/// hover cue. Our cells are always locked (see XUiC_BeyondStorage_ItemGrid.LockCells, needed to
/// block pickup/drag and prevent item duplication), so that X would show on every hover.
/// Overriding "islocked" to always report false to the view layer hides it without touching the
/// real IsLocked field, which XUiC_ItemStack.Update reads directly (not via this binding) to gate
/// interaction — so pickup/drag stays blocked exactly as before. "cancel" is the only thing in this
/// template bound to islocked alone, so nothing else here is affected.
/// </summary>
[Preserve]
public class XUiC_BeyondStorage_ItemStack : XUiC_ItemStack
{
    [PublicizedFrom(EAccessModifier.Protected)]
    public override bool GetBindingValueInternal(ref string value, string bindingName)
    {
        if (bindingName == "islocked")
        {
            value = "False";
            return true;
        }

        return base.GetBindingValueInternal(ref value, bindingName);
    }
}
