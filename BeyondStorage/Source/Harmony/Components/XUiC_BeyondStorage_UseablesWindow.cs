using BeyondStorage.Game.UI;
using UnityEngine.Scripting;

namespace BeyondStorage.Harmony.Components;

[Preserve]
public class XUiC_BeyondStorage_UseablesWindow : XUiController
{
    [PublicizedFrom(EAccessModifier.Private)]
    public XUiC_BeyondStorage_UseablesGrid useablesGrid;

    public override void Init()
    {
        base.Init();
        useablesGrid = base.GetChildByType<XUiC_BeyondStorage_UseablesGrid>();
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);
    }

    [PublicizedFrom(EAccessModifier.Protected)]
    public override bool GetBindingValueInternal(ref string value, string bindingName)
    {
#if DEBUG
        //const string d_MethodName = nameof(GetBindingValueInternal);
#endif
        switch (bindingName)
        {
            case "bs_is_player_backpack_only":
                value = WindowStateManager.IsOnlyPlayerBackpackOpen();
#if DEBUG
                //ModLogger.DebugLog($"{d_MethodName}: bindingName={bindingName}, value={value}");
#endif
                return true;  // We've handled it

            default:
                return base.GetBindingValueInternal(ref value, bindingName);
        }
    }
}