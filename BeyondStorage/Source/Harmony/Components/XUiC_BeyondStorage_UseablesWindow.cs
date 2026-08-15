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
}