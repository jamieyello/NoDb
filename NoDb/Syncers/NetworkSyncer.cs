namespace NoDb.Syncers;

public class NetworkSyncer : Syncer
{
    NetworkSyncerConfig Config => (NetworkSyncerConfig)_config;

    public NetworkSyncer(NetworkSyncerConfig config) : base(config)
    {
    }
}