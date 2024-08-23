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
}