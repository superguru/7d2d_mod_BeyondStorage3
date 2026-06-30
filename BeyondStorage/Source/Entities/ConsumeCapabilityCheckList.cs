using System.Collections.Generic;
using BeyondStorage.Infrastructure;

namespace BeyondStorage.Entities;

internal static class ConsumeCapabilityCheckList
{
    private static readonly object s_lock = new();

    internal delegate (bool Applies, bool CanToggle) CanToggleConsumeCheck(object a);

    private static readonly HashSet<CanToggleConsumeCheck> s_canToggleConsumeEvents = [];

    internal static void AddCheck(CanToggleConsumeCheck check)
    {
        s_canToggleConsumeEvents.Add(check);
    }

    internal static bool AnyPasses(object a)
    {
#if DEBUG
        const string d_MethodName = nameof(AnyPasses);
#endif

#if DEBUG
        int checkedCount = 0;
        int appliesCount = 0;
#endif

        if (a == null)
        {
#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: checks auto-failed for null object");
#endif
        }

        bool result = false;

        foreach (var check in s_canToggleConsumeEvents)
        {
#if DEBUG
            checkedCount++;
#endif
            var (Applies, CanToggle) = check(a);
            if (Applies)
            {
#if DEBUG
                appliesCount++;
#endif
                if (CanToggle)
                {
                    result = true;
                    break;
                }
            }
        }

#if DEBUG
        //ModLogger.DebugLog($"{d_MethodName}: Check result={result}, checkedCount={checkedCount}, appliesCount={appliesCount}, a={a.GetType()}");
#endif

        return result;
    }
}