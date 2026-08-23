using BeyondStorage.Game.UI;
using UnityEngine.Scripting;

namespace BeyondStorage.Harmony.Components;

[Preserve]
public class XUiC_BeyondStorage_UseablesWindow : XUiController
{
    // Matches StorageContextFactory's context cache TTL — no point refreshing more often than
    // the underlying storage counts can actually change.
    private const float REFRESH_INTERVAL_SECONDS = 0.5f;
    private float _refreshTimer;

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

        _refreshTimer += _dt;
        if (_refreshTimer < REFRESH_INTERVAL_SECONDS)
        {
            return;
        }
        _refreshTimer = 0f;

        if (WindowStateManager.IsOnlyPlayerBackpackOpenInternal())
        {
            return;
        }

        useablesGrid?.RefreshTopItems();
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