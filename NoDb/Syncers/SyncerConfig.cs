namespace NoDb.Syncers;

// todo: make this abstract somehow
public class SyncerConfig
{
    List<SyncerConfig> Configs { get; init; } = new();
    /// <summary> If set to true, this will load the object from the source on initialization. </summary>
    public virtual bool Load { get; set; }
    public virtual bool SupportsPull => throw new NotImplementedException();

    protected SyncerConfig() { }
    SyncerConfig(IEnumerable<SyncerConfig> configs, SyncerConfig add) =>
        Configs.AddRange(configs.Append(add));

    #region Helper Functions
    public SyncerConfig WithSyncer(SyncerConfig config) =>
        new(Configs, config);

    /// <summary> Will create a <see cref="FileSyncer"/> that will load and keep this object syncronyzed to an individual file on the drive. </summary>
    public static SyncerConfig FileSync(string file_path) =>
        new FileSyncerConfig { FilePath = file_path };
    /// <summary> Returns a new <see cref="SyncerConfig"/> with settings for a <see cref="FileSyncer"/> appended. </summary>
    public SyncerConfig WithFileSync(string file_path) =>
        new(Configs, new FileSyncerConfig { FilePath = file_path });

    /// <summary> Will create a <see cref="NoDbSyncer"/> that will load and keep this object syncronized to a NoDb Database. </summary>
    /// <param name="connection_string">The database connection string.</param>
    public static SyncerConfig NoDbSync(string connection_string) =>
        new NoDbSyncerConfig { ConnectionString = connection_string };
    /// <summary> Returns a new <see cref="SyncerConfig"/> with settings for a <see cref="NoDbSyncer"/> appended. </summary>
    public SyncerConfig WithDbSync(string connection_string) =>
        new(Configs, new NoDbSyncerConfig { ConnectionString = connection_string });
    #endregion

    protected virtual Syncer GetSyncer() => 
        throw new NotImplementedException();

    public IEnumerable<Syncer> GetSyncers() =>
        Configs.Select(x => x.GetSyncer()).Append(GetSyncer());
}
