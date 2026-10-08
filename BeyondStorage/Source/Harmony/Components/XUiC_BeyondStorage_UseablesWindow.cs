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
    /// this window's visibility condition (backpack-only) holds. We still bail out while any text
    /// input is being edited (e.g. the craft-count field) so a typed digit isn't stolen by a slot.
    /// </summary>
    private void PollHotkeys()
    {
        if (xui.playerUI.windowManager.IsInputActive())
        {
            return;
        }

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
    /// Handles clicks on a cell: every click shows the item info panel (see ShowItemInfo for how the
    /// action list is kept honest), and a second click within the double-click window additionally
    /// triggers TryUseSlot. Cells are locked
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

            ShowItemInfo(controllers[i]);

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
    /// Shows the item info panel for a Useables cell, populated with the same action list a real
    /// backpack slot would get (<see cref="XUiC_ItemActionList.ItemActionListTypes.Item"/>), so
    /// clicking a cell lists that item's actions exactly like clicking the item in your backpack.
    ///
    /// This goes through <c>SetItemStack</c> (what <c>XUiC_ItemStack.updateItemInfoWindow</c> calls
    /// for a real backpack cell) rather than <c>SetInfo</c> directly, so the panel keeps a
    /// <c>selectedItemStack</c> to re-render itself from. The Stats/Description buttons only flip
    /// <c>showStats</c> and set <c>IsDirty</c>; the actual page refresh happens in
    /// <c>XUiC_ItemInfoWindow.Update</c>, which re-runs <c>SetItemStack(selectedItemStack)</c>.
    /// With <c>selectedItemStack</c> null every one of those branches is skipped, so the button
    /// highlights while the panel keeps showing the previously selected item's content.
    ///
    /// The resulting buttons are bound straight to the cell controller, which only ever holds a
    /// display stack, so <c>XUiC_ItemActionEntry_Ext</c> intercepts activation and routes Use back
    /// through <c>TryUseSlot</c> and Drop through storage. Without that interception the panel
    /// would apply effects for free and drop items into the world from nothing.
    /// </summary>
    private static void ShowItemInfo(XUiC_ItemStack cellController)
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

        infoWindow.SetItemStack(cellController, _makeVisible: true);
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

            case "bs_show_useables":
                value = WindowStateManager.ShowUseablesWindow();

#if DEBUG
                //ModLogger.DebugLog($"{d_MethodName}: bindingName={bindingName}, value={value}");
#endif
                return true;  // We've handled it

            default:
                return base.GetBindingValueInternal(ref value, bindingName);
        }
    }
}