using System.IO;
using BeyondStorage.Entities;

#if DEBUG
using BeyondStorage.Infrastructure;
#endif

namespace BeyondStorage.Multiplayer;

public class NetPackageInventoryNetworkStateChange : NetPackage
{
    private Vector3i _position;
    private bool _isDisabledForInventoryNetwork;

    public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

    public NetPackageInventoryNetworkStateChange Setup(Vector3i position, bool isDisabledForInventoryNetwork)
    {
        _position = position;
        _isDisabledForInventoryNetwork = isDisabledForInventoryNetwork;
        return this;
    }

    public override void write(PooledBinaryWriter _writer)
    {
        base.write(_writer);
        var writer = (BinaryWriter)_writer;
        StreamUtils.Write(writer, _position);
        writer.Write(_isDisabledForInventoryNetwork);
#if DEBUG
        ModLogger.DebugLog($"NetPackageInventoryNetworkStateChange write: pos {_position}, isDisabledForInventoryNetwork {_isDisabledForInventoryNetwork}");
#endif
    }

    public override void read(PooledBinaryReader reader)
    {
        _position = StreamUtils.ReadVector3i(reader);
        _isDisabledForInventoryNetwork = reader.ReadBoolean();
#if DEBUG
        ModLogger.DebugLog($"NetPackageInventoryNetworkStateChange read: pos {_position}, isDisabledForInventoryNetwork {_isDisabledForInventoryNetwork}");
#endif
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        if (world == null)
        {
            return;
        }
        BlockInventoryNetworkStates.ApplyServerSideChange(_position, _isDisabledForInventoryNetwork);
    }

    public override int GetLength()
    {
        const int intSize = 4;
        const int posSize = 3 * intSize;
        return 1 + posSize + sizeof(bool);
    }
}
