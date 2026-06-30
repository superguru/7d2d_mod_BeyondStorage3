namespace BeyondStorage.Entities;

internal static class ConsumeCapabilityChecker_TEFeatureStorage
{
    internal static (bool Applies, bool CanToggle) CanToggleConsume(object a)
    {
        (bool Applies, bool ShouldToggle) canToggle = (false, false);

        if (a is not TEFeatureStorage storage)
        {
            return canToggle;
        }

        canToggle.Applies = true;
        canToggle.ShouldToggle = storage.bPlayerStorage;

        return canToggle;
    }
}