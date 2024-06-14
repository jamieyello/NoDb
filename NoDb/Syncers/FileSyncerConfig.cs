
namespace NoDb.Syncers;

/// <summary>Will create a <see cref="FileSyncer"/> that will load and keep this object syncronyzed to an individual file on the drive.</summary>
public class FileSyncerConfig : SyncerConfig
{
    /// <summary> The path to the file that will be synced. This and all directories will be created if it does not exist. </summary>
    public required string FilePath { get; init; }

    internal FileSyncerConfig() { 
        Load = true;
    }

    internal override Syncer GetSyncer() => 
        new FileSyncer(this);
}