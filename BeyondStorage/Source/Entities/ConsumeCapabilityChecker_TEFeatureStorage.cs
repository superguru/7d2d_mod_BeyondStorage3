namespace BeyondStorage.Entities;

internal static class ConsumeCapabilityChecker_TEFeatureStorage
{
    internal static bool CanToggleConsume(object a)
    {
        if (a is not TEFeatureStorage storage)
        {
            return false;
        }

        return storage.bPlayerStorage;
    }
}