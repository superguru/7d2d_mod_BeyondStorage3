using System.IO;
using BeyondStorage.Entities;

#if DEBUG
using BeyondStorage.Infrastructure;
#endif

namespace BeyondStorage.Multiplayer;

public class NetPackageConsumeStateChange : NetPackage
{
    private Vector3i _position;
    private bool _isConsumeOff;

    public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

    public NetPackageConsumeStateChange Setup(Vector3i position, bool isConsumeOff)
    {
        _position = position;
        _isConsumeOff = isConsumeOff;
        return this;
    }

    public override void write(PooledBinaryWriter _writer)
    {
        base.write(_writer);
        var writer = (BinaryWriter)_writer;
        StreamUtils.Write(writer, _position);
        writer.Write(_isConsumeOff);
#if DEBUG
        ModLogger.DebugLog($"NetPackageConsumeStateChange write: pos {_position}, isConsumeOff {_isConsumeOff}");
#endif
    }

    public override void read(PooledBinaryReader reader)
    {
        _position = StreamUtils.ReadVector3i(reader);
        _isConsumeOff = reader.ReadBoolean();
#if DEBUG
        ModLogger.DebugLog($"NetPackageConsumeStateChange read: pos {_position}, isConsumeOff {_isConsumeOff}");
#endif
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        if (world == null)
        {
            return;
        }
        BlockConsumeStates.ApplyServerSideChange(_position, _isConsumeOff);
    }

    public override int GetLength()
    {
        const int intSize = 4;
        const int posSize = 3 * intSize;
        return 1 + posSize + sizeof(bool);
    }
}
