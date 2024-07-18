namespace NoDb.Syncers;

internal class NoDbSyncerConfig : SyncerConfig
{
    public required string ConnectionString { get; init; }

    protected override Syncer GetSyncer() =>
        new NoDbSyncer(this);
}
