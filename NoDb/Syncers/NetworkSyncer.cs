using NoDb.Networking;
using SlothSerializer;

namespace NoDb.Syncers;

/// <summary>
/// This is intended to be an intermediate class to help test
/// the LobbySyncer class. The final designs will have no concept
/// of server and client.
/// </summary>
public class NetworkSyncer : Syncer
{
    NetworkSyncerConfig Config => (NetworkSyncerConfig)_config;

    public NetworkSyncer(NetworkSyncerConfig config) : base(config)
    {
    }

    public override Task<ConnectionResult> Connect()
    {
        return base.Connect();
    }

    public override Task<T?> FullLoad<T>(T default_value) where T : default
    {
        return base.FullLoad(default_value);
    }

    public override Task Push(BitBuilderDiff diff)
    {
        return base.Push(diff);
    }

    public override Task ClosingPush(BitBuilderDiff diff)
    {
        return base.ClosingPush(diff);
    }

    public override Task<BitBuilderDiff> Pull(BitBuilderDiff diff)
    {
        return base.Pull(diff);
    }
}