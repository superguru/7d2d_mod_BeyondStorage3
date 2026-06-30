using System.Collections.Generic;
using BeyondStorage.Infrastructure;

namespace BeyondStorage.Entities;

internal static class ConsumeCapabilityCheckList
{
    private static readonly object s_lock = new();

    internal delegate bool CanToggleConsumeCheck(object a);

    private static readonly HashSet<CanToggleConsumeCheck> s_canToggleConsumeEvents = [];

    internal static void AddCheck(CanToggleConsumeCheck check)
    {
        lock (s_lock)
        {
            s_canToggleConsumeEvents.Add(check);
        }
    }

    internal static bool AnyPasses(object a)
    {
#if DEBUG
        const string d_MethodName = nameof(AnyPasses);
#endif

        if (a == null)
        {
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: checks auto-failed for null object");
#endif
            return false;
        }

        lock (s_lock)
        {
            foreach (var check in s_canToggleConsumeEvents)
            {
                bool canToggle = check(a);

                if (canToggle)
                {
                    return true;
                }
            }

            return false;
        }
    }
}