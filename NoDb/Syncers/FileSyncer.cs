using System.Runtime.Versioning;
using SlothSerializer;
using SlothSerializer.Internal;

namespace NoDb.Syncers;

public class FileSyncer : Syncer
{
    FileSyncerConfig Config => (FileSyncerConfig)_config;

    public FileSyncer(FileSyncerConfig config) : base(config) { }

    public override async Task<T?> FullLoad<T>(T default_value) where T : default {
        if (File.Exists(Config.FilePath)) {
            var bb_result = new BitBuilderBuffer();
            await bb_result.ReadFromDiskAsync(Config.FilePath);
            var t_result = bb_result.GetReader().Read<T>();
            return t_result;
        }
        else {
            var directory = Path.GetDirectoryName(Config.FilePath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            var bb = new BitBuilderBuffer();
            bb.Append(default_value);
            await bb.WriteToDiskAsync(Config.FilePath);
            return default_value;
        }
    }

    public override Task<BinaryDiff> Pull(BinaryDiff diff)
    {
        return base.Pull(diff);
    }

    public override async Task Push(BinaryDiff diff) {
        using var fs = new FileStream(Config.FilePath, FileMode.Open);
        await diff.ApplyToAsync(fs);
    }

    public override Task ClosingPush(BinaryDiff diff)
    {
        return base.ClosingPush(diff);
    }
}