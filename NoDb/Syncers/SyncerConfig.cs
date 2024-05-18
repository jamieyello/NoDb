namespace NoDb.Syncers;

public class SyncerConfig
{
    internal List<SyncerConfig> _configs { get; init; } = new();
    /// <summary> If set to true, this will load the object from the source on initialization. </summary>
    public bool Load { get; init; }

    internal SyncerConfig() { }
    SyncerConfig(IEnumerable<SyncerConfig> configs, SyncerConfig add) =>
        _configs.AddRange(configs.Append(add));

    #region Helper Functions
    /// <summary>Will create a <see cref="FileSyncer"/> that will load and keep this object syncronyzed to an individual file on the drive.</summary>
    public static SyncerConfig FileSync(string file_path) =>
        new FileSyncerConfig { FilePath = file_path };
    /// <summary> Returns a new <see cref="SyncerConfig"/> with settings for a <see cref="FileSyncer"/> appended. </summary>
    public SyncerConfig WithFileSync(string file_path) =>
        new(_configs, new FileSyncerConfig { FilePath = file_path });

    /// <summary> Will create a <see cref="NoDbSyncer"/> that will load and keep this object syncronized to a NoDb Database. </summary>
    /// <param name="connection_string">The database connection string.</param>
    public static SyncerConfig NoDbSync(string connection_string) =>
        new NoDbSyncerConfig { ConnectionString = connection_string };
    /// <summary> Returns a new <see cref="SyncerConfig"/> with settings for a <see cref="NoDbSyncer"/> appended. </summary>
    public SyncerConfig WithDbSync(string connection_string) =>
        new(_configs, new NoDbSyncerConfig { ConnectionString = connection_string });
    #endregion

    internal virtual Syncer GetSyncer() => throw new NotImplementedException();
}
