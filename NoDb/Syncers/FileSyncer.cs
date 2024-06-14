using System.Runtime.Versioning;
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

    public override async Task<BitBuilderBuffer> FullLoad<T>(T default_value)
    {
        var result = new BitBuilderBuffer();
        await result.ReadFromDiskAsync(Config.FilePath);
        return result;
    }

    public override Task<BinaryDiff> Pull(BinaryDiff diff)
    {
        return base.Pull(diff);
    }

    public override Task<bool> Push(BinaryDiff diff)
    {
        return base.Push(diff);
    }

    public override Task ClosingPush(BinaryDiff diff)
    {
        return base.ClosingPush(diff);
    }
}