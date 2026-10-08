using HarmonyLib;

namespace BeyondStorage.Harmony.Extends;

/// <summary>
/// 3.3 regression fix: the Useables window used to apply an item's action, and stopped doing so
/// in 3.3 even though it still removed the unit from storage.
///
/// Up to 3.2, <c>ItemActionEntryUse.UseItemWithAnimationCoroutine</c> handed the single
/// <see cref="ItemStack"/> it was about to use straight to
/// <c>Inventory.SimulateActionExecution(actionIdx, itemStack, onComplete)</c>, which parked it in
/// the toolbelt's DUMMY slot. Nothing on that path cared where the stack actually lived, so a
/// synthetic cell that merely *displayed* an aggregated storage count worked unchanged.
///
/// 3.3 replaced that overload with <c>SimulateActionExecution(actionIdx, ItemStackGrid, int)</c>,
/// and the coroutine now has to name a real grid first. It derives one from the cell's
/// <c>StackLocation</c> (see the switch in <c>UseItemWithAnimationCoroutine</c>:
/// <c>StackLocationTypes.Backpack =&gt; xui.PlayerInventory.Backpack.ItemGrid</c>, and so on),
/// then calls <c>Hand.SimulateActionExecution</c>, which starts with
/// <c>ItemStack itemStack = _sourceGrid[_sourceIdx]; if (Item == null || itemStack.IsEmpty())
/// yield break;</c>.
///
/// Beyond Storage's cells report <c>StackLocation = Backpack</c> (deliberately — it makes
/// vanilla treat them as player-owned stacks for the action list) and carry a <c>SlotNumber</c> in
/// the 0-5 Useables range. Vanilla therefore resolved the player's <i>real</i> backpack
/// <c>ItemStackGrid</c> and indexed it with a Useables slot number. Those slots are usually empty,
/// so <c>Hand.SimulateActionExecution</c> bailed out before doing anything: the eat/drink/heal
/// animation, buffs and XP never ran, while <c>TryUseSlot</c> had already removed the unit from
/// storage. (If those backpack slots happened to be occupied it would have consumed those items
/// instead.)
///
/// There is no <c>StackLocation</c> value that fixes this: every real value maps to one of the
/// player's own grids, and the value 3.3 added for "no location" (<c>StackLocationTypes.None</c>)
/// maps to <c>null</c>, which skips the simulation altogether. So the Useables grid owns a small
/// real <see cref="ItemStackGrid"/>, parks the single unit it is about to use in it, and arms this
/// patch; the very next <c>SimulateActionExecution</c> call is redirected to that grid instead of
/// the backpack. Everything downstream — transient hold, animation, <c>ItemActionEat.consume</c>,
/// buffs, XP, jar refund, <c>SwitchBack</c> — is vanilla's, untouched.
/// </summary>
[HarmonyPatch(typeof(Inventory))]
internal static class Inventory_Ext
{
    private static readonly object s_lockObject = new();

    // Armed by XUiC_BeyondStorage_UseablesGrid immediately before it calls
    // ItemActionEntryUse.OnActivated, and consumed by the prefix below.
    private static ItemStackGrid s_sourceGrid;
    private static int s_sourceIdx = -1;

    /// <summary>
    /// Redirects the next <c>SimulateActionExecution</c> call to <paramref name="grid"/>,
    /// <paramref name="slotIndex"/> instead of the caller-supplied source.
    /// </summary>
    public static void ArmSyntheticUseSource(ItemStackGrid grid, int slotIndex)
    {
        lock (s_lockObject)
        {
            s_sourceGrid = grid;
            s_sourceIdx = slotIndex;
        }
    }

    /// <summary>
    /// Clears any armed redirect. Safe to call when nothing is armed, and safe to call when the
    /// armed value was already consumed by the prefix (i.e. when OnActivated reached
    /// SimulateActionExecution synchronously).
    /// </summary>
    public static void DisarmSyntheticUseSource()
    {
        lock (s_lockObject)
        {
            s_sourceGrid = null;
            s_sourceIdx = -1;
        }
    }

    // Vanilla parameter names (_sourceGrid / _sourceIdx) are reused deliberately so Harmony binds
    // by name rather than falling back to position.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Inventory.SimulateActionExecution), new[] { typeof(int), typeof(ItemStackGrid), typeof(int) })]
#if DEBUG
    [HarmonyDebug]
#endif
    private static void Inventory_SimulateActionExecution_Prefix(ref ItemStackGrid _sourceGrid, ref int _sourceIdx)
    {
        ItemStackGrid armedGrid;
        int armedIdx;

        lock (s_lockObject)
        {
            armedGrid = s_sourceGrid;
            if (armedGrid == null)
            {
                // Not ours — every vanilla caller (toolbelt, backpack, loot container) passes
                // through untouched.
                return;
            }

            armedIdx = s_sourceIdx;

            // Consumed here rather than left armed: OnActivated reaches this synchronously (via
            // StartCoroutine running up to the first yield), so by the time TryUseSlot gets control
            // back the redirect has been used. Clearing it also means a use that never gets this
            // far can't leak into an unrelated later call.
            s_sourceGrid = null;
            s_sourceIdx = -1;
        }

        _sourceGrid = armedGrid;
        _sourceIdx = armedIdx;
    }
}