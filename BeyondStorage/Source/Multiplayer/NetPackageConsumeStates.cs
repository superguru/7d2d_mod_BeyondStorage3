using System.Collections.Generic;
using System.IO;
using BeyondStorage.Entities;

#if DEBUG
using BeyondStorage.Infrastructure;
#endif

namespace BeyondStorage.Multiplayer;

public class NetPackageConsumeStates : NetPackage
{
    private List<Vector3i> _disabledBlocks = [];

    public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

    public NetPackageConsumeStates Setup(List<Vector3i> disabledBlocks)
    {
        _disabledBlocks = disabledBlocks;
        return this;
    }

    public override void write(PooledBinaryWriter _writer)
    {
        base.write(_writer);
        var writer = (BinaryWriter)_writer;
        writer.Write(_disabledBlocks.Count);
        foreach (var pos in _disabledBlocks)
        {
            StreamUtils.Write(writer, pos);
        }
#if DEBUG
        ModLogger.DebugLog($"NetPackageConsumeStates write: {_disabledBlocks.Count} disabled blocks");
#endif
    }

    public override void read(PooledBinaryReader reader)
    {
        int count = reader.ReadInt32();
        _disabledBlocks = new List<Vector3i>(count);
        for (int i = 0; i < count; i++)
        {
            _disabledBlocks.Add(StreamUtils.ReadVector3i(reader));
        }
#if DEBUG
        ModLogger.DebugLog($"NetPackageConsumeStates read: {count} disabled blocks");
#endif
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        if (world == null)
        {
            return;
        }
        BlockConsumeStates.ApplyFromServer(_disabledBlocks);
    }

    public override int GetLength()
    {
        const int intSize = 4;
        const int posSize = 3 * intSize;
        return 1 + intSize + posSize * _disabledBlocks.Count;
    }
}
