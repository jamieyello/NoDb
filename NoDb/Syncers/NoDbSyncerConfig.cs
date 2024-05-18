namespace NoDb.Syncers;

internal class NoDbSyncerConfig : SyncerConfig
{
    public required string ConnectionString { get; init; }

    internal override Syncer GetSyncer() =>
        new NoDbSyncer(this);
}
