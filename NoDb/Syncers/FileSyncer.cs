using SlothSerializer;
using SlothSerializer.Internal;

namespace NoDb.Syncers;

public class FileSyncer : Syncer
{
    FileSyncerConfig Config => (FileSyncerConfig)_config;

    public FileSyncer(FileSyncerConfig config) : base(config) {
        FileAttributes attr = File.GetAttributes(Config.FilePath);
        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            throw new Exception($"Given file path for {nameof(FileSyncer)} is a directory.");

        Directory.CreateDirectory(Path.GetDirectoryName(Config.FilePath) ?? throw new Exception($"Failed to get directory name for {Config.FilePath}"));
    }

    public override async Task<BitBuilderBuffer> FullLoad()
    {
        return await base.FullLoad();
    }
}