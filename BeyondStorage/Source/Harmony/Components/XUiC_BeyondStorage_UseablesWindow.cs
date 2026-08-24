using BeyondStorage.Game.UI;
using UnityEngine;
using UnityEngine.Scripting;

namespace BeyondStorage.Harmony.Components;

[Preserve]
public class XUiC_BeyondStorage_UseablesWindow : XUiController
{
    // Matches StorageContextFactory's context cache TTL — no point refreshing more often than
    // the underlying storage counts can actually change.
    private const float REFRESH_INTERVAL_SECONDS = 0.5f;
    private const int SLOT_COUNT = 6;
    private const float DOUBLE_CLICK_WINDOW_SECONDS = 0.35f;

    private float _refreshTimer;
    private readonly float[] _lastClickTime = new float[SLOT_COUNT];

    [PublicizedFrom(EAccessModifier.Private)]
    public XUiC_BeyondStorage_UseablesGrid useablesGrid;

    public override void Init()
    {
        base.Init();
        useablesGrid = base.GetChildByType<XUiC_BeyondStorage_UseablesGrid>();
    }

    public override void OnOpen()
    {
        base.OnOpen();

        WindowStateManager.OnUseablesWindowOpening(this);
        RefreshBindings();
    }

    public override void OnClose()
    {
        base.OnClose();

        WindowStateManager.OnUseablesWindowClosing(this);
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);

        if (!viewComponent.isVisible)
        {
            return;
        }

        PollHotkeys();
        PollDoubleClick();

        _refreshTimer += _dt;
        if (_refreshTimer < REFRESH_INTERVAL_SECONDS)
        {
            return;
        }
        _refreshTimer = 0f;

        useablesGrid?.RefreshTopItems();
    }

    /// <summary>
    /// Number keys 1-6 map straight to slots 1-6 (Heal row, then Food/Drink row). Plain digits are
    /// safe here since the toolbelt hotkeys they'd otherwise trigger aren't active while this
    /// window's visibility condition (backpack-only) holds.
    /// </summary>
    private void PollHotkeys()
    {
        for (int slotIndex = 0; slotIndex < SLOT_COUNT; slotIndex++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + slotIndex)))
            {
                useablesGrid?.TryUseSlot(slotIndex);
            }
        }
    }

    /// <summary>
    /// Detects a double-click on a cell as an alternate trigger for the same TryUseSlot action.
    /// Cells are locked (see XUiC_BeyondStorage_UseablesGrid.LockCells) so the vanilla click
    /// pipeline never fires, but XUiC_ItemStack.isOver is still updated on hover regardless of
    /// lock state, so hover+mouse-button state read here independently is all that's needed.
    /// </summary>
    private void PollDoubleClick()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        var controllers = useablesGrid?.GetItemStackControllers();
        if (controllers == null)
        {
            return;
        }

        for (int i = 0; i < controllers.Length; i++)
        {
            if (!controllers[i].isOver)
            {
                continue;
            }

            float now = Time.time;
            bool isDoubleClick = (now - _lastClickTime[i]) <= DOUBLE_CLICK_WINDOW_SECONDS;

            if (isDoubleClick)
            {
                _lastClickTime[i] = 0f; // reset so a 3rd click doesn't chain into another double-click
                useablesGrid.TryUseSlot(i);
            }
            else
            {
                _lastClickTime[i] = now;
            }

            break; // only one cell can be hovered at a time
        }
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