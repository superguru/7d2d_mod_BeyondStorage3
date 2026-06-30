using System.IO;

namespace BeyondStorage.Source.Game.Files;

internal static class GamePathProvider
{
    internal static string SaveGameDir { get; set; } = null;

    internal static void Init()
    {
        SaveGameDir = GameIO.GetSaveGameDir();
    }

    internal static void Cleanup()
    {
        SaveGameDir = null;
    }

    internal static string GetFullSaveGamePathName(string filename)
    {
        if (SaveGameDir == null)
        {
            return null;
        }

        return Path.Combine(SaveGameDir, filename);
    }
}
