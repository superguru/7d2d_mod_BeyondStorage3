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

        // Runs regardless of visibility so a stuck busy flag still recovers even if the player
        // closed the backpack while a use was pending.
        useablesGrid?.CheckStuckUseWatchdog();

        if (!viewComponent.isVisible)
        {
            return;
        }

        PollHotkeys();
        PollClicks();

        _refreshTimer += _dt;
        if (_refreshTimer < REFRESH_INTERVAL_SECONDS)
        {
            return;
        }
        _refreshTimer = 0f;

        useablesGrid?.RefreshGridItems();
    }

    /// <summary>
    /// Number keys 1-6 (top row) map straight to slots 1-6 (Heal row, then Food/Drink row). Plain
    /// digits are safe here since the toolbelt hotkeys they'd otherwise trigger aren't active while
    /// this window's visibility condition (backpack-only) holds.
    /// </summary>
    private void PollHotkeys()
    {
        for (int slotIndex = 0; slotIndex < SLOT_COUNT; slotIndex++)
        {
            var alphaKey = (KeyCode)((int)KeyCode.Alpha1 + slotIndex);

            if (Input.GetKeyDown(alphaKey))
            {
                useablesGrid?.TryUseSlot(slotIndex);
            }
        }
    }

    /// <summary>
    /// Handles clicks on a cell: every click shows the item info panel (read-only — see
    /// ShowReadOnlyItemInfo for why it's NOT XUiC_ItemStack.HandleItemInspect()), and a second
    /// click within the double-click window additionally triggers TryUseSlot. Cells are locked
    /// (see XUiC_BeyondStorage_UseablesGrid.LockCells) so the vanilla click pipeline that would
    /// normally do both of these never runs, but XUiC_ItemStack.isOver is still updated on hover
    /// regardless of lock state, so hover+mouse-button state read here independently is all that's
    /// needed.
    /// </summary>
    private void PollClicks()
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

            ShowReadOnlyItemInfo(controllers[i]);

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

    /// <summary>
    /// Shows the item info panel without exposing any default vanilla item actions (Drop, Use,
    /// etc.). XUiC_ItemStack.HandleItemInspect() -> XUiC_ItemInfoWindow.SetItemStack() both go
    /// through SetInfo(..., ItemActionListTypes.Item), which populates the panel's action list
    /// with FULLY FUNCTIONAL buttons bound directly to the cell controller passed in — for our
    /// synthetic cells that's a real duplication bug: e.g. the panel's "Drop" button (or its
    /// keyboard shortcut) drops a real item stack on the ground while our storage-backed count is
    /// never touched, and its "Use" button applies the item's effect and decrements only the
    /// cell's own display count, bypassing TryUseSlot's storage removal entirely. Calling SetInfo
    /// directly with ItemActionListTypes.None gets the same read-only display (name/stats/icon)
    /// with an empty action list, so 1-6 / double-click (TryUseSlot) are the only way to act on
    /// these cells.
    /// </summary>
    private static void ShowReadOnlyItemInfo(XUiC_ItemStack cellController)
    {
        var itemStack = cellController?.ItemStack;
        if (itemStack == null || itemStack.IsEmpty())
        {
            return;
        }

        var infoWindow = cellController.InfoWindow;
        if (infoWindow == null)
        {
            return;
        }

        infoWindow.ClearSelectedStacks();
        infoWindow.makeVisible(true);
        infoWindow.SetInfo(itemStack, cellController, XUiC_ItemActionList.ItemActionListTypes.None);
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