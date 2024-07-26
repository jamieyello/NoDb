using NoDb.Syncers;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NoDb;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary> Syncs an object with a file. </summary>
public sealed class FileObject<T> : SyncedObject<T> {
    public FileObject(string file_path, T? default_value = default, bool delete_existing = false) 
        : base(new FileSyncerConfig() { FilePath = file_path, DeleteExisting = delete_existing }, default_value) { }
}