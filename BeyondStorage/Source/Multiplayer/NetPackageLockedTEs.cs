using System.Collections.Generic;
using System.IO;

#if DEBUG
using BeyondStorage.Infrastructure;
#endif

namespace BeyondStorage.Multiplayer;

public class NetPackageLockedTEs : NetPackage
{
    public int EntryCount;
    public Dictionary<Vector3i, int> LockedTileEntities;

    public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

    public NetPackageLockedTEs Setup(Dictionary<Vector3i, int> lockedTEs)
    {
        LockedTileEntities = new Dictionary<Vector3i, int>(lockedTEs);
        EntryCount = LockedTileEntities.Count;
        return this;
    }

    public override void write(PooledBinaryWriter _writer)
    {
        base.write(_writer);

        var binaryWriter = ((BinaryWriter)_writer);

        binaryWriter.Write(LockedTileEntities.Count);
        foreach (var kvp in LockedTileEntities)
        {
            StreamUtils.Write(binaryWriter, kvp.Key);
            binaryWriter.Write(kvp.Value);
#if DEBUG
            ModLogger.DebugLog($"pos {kvp.Key}, value {kvp.Value}");
#endif
        }
    }

    public override void read(PooledBinaryReader binaryReader)
    {
        EntryCount = binaryReader.ReadInt32();
        LockedTileEntities = new Dictionary<Vector3i, int>();
        for (var i = 0; i < EntryCount; i++)
        {
            var pos = StreamUtils.ReadVector3i(binaryReader);
            var lockingEntityId = binaryReader.ReadInt32();
#if DEBUG
            ModLogger.DebugLog($"tePOS {pos}; lockingEntityId {lockingEntityId}");
#endif
            LockedTileEntities.Add(pos, lockingEntityId);
        }
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        // skip if we don't have a valid world yet
        if (world == null)
        {
            return;
        }

        TileEntityLocks.UpdateLockedTEs(LockedTileEntities);
    }
}