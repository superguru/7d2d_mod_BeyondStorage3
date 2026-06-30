using System.Collections.Concurrent;
using BeyondStorage.Infrastructure;
using BeyondStorage.Source.Persistence;

namespace BeyondStorage.Source.Game.Files;

internal static class BlockConsumeStatePersistence
{
    private static readonly object s_lock = new();

    private const int FileVersion = 0x0304;
    private const string BlockConsumeOffFile = "bs_disabled_blocks.dat";
    private const string BlockRecordTag = "v";
    private const string BlockFieldName = "pos";

    internal static string GetBlockConsumeStateFilePath()
    {
        return GamePathProvider.GetFullSaveGamePathName(BlockConsumeOffFile);
    }

    internal static void LoadDisabledBlocks(ConcurrentDictionary<Vector3i, byte> disabledBlocks)
    {
#if DEBUG
        const string d_MethodName = nameof(LoadDisabledBlocks);
#endif
        lock (s_lock)
        {
            disabledBlocks.Clear();

            var blockFile = new StructuredFile();
            string fileName = GetBlockConsumeStateFilePath();

#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: fileName={fileName}");
#endif

            blockFile.ReadFile(fileName);

#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: ver:{blockFile.Version}, tags:{blockFile.RecordTagCount}, total records:{blockFile.RecordCount} in {fileName}");
#endif

            foreach (var record in blockFile.GetRecordsByTag(BlockRecordTag))
            {
                var field = record.GetField(BlockFieldName);
                if (field == null)
                {
#if DEBUG
                    ModLogger.DebugLog($"{d_MethodName}: skipping record with no {BlockFieldName} field");
#endif
                    continue;
                }

                var pos = field.AsVector3i;
                if (pos == null)
                {
#if DEBUG
                    ModLogger.DebugLog($"{d_MethodName}: skipping record with unparseable position value='{field}'");
#endif
                    continue;
                }

#if DEBUG
                ModLogger.DebugLog($"{d_MethodName}: loaded disabled block at {pos.Value}");
#endif
                disabledBlocks[pos.Value] = 0;
            }
        }
    }

    internal static void SaveDisabledBlocks(ConcurrentDictionary<Vector3i, byte> disabledBlocks)
    {
#if DEBUG
        const string d_MethodName = nameof(SaveDisabledBlocks);
#endif
        lock (s_lock)
        {
            string fileName = GetBlockConsumeStateFilePath();
            if (fileName == null)
            {
#if DEBUG
                ModLogger.DebugLog($"{d_MethodName}: cannot save, no save path available");
#endif
                return;
            }

            var blockFile = new StructuredFile();
            blockFile.SetMeta(FileVersion);

            foreach (var pos in disabledBlocks.Keys)
            {
                var record = new StructuredRecord(BlockRecordTag);
                record.SetField(BlockFieldName, $"{pos.x},{pos.y},{pos.z}");
                blockFile.AddRecord(record);
            }

#if DEBUG
            ModLogger.DebugLog($"{d_MethodName}: saving {disabledBlocks.Count} disabled blocks to {fileName}");
#endif

            blockFile.WriteFile(fileName);
        }
    }
}
