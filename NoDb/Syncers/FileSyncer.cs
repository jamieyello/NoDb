using SlothSerializer;
using SlothSerializer.DiffTracking;

namespace NoDb.Syncers;

public class FileSyncer : Syncer
{
    FileSyncerConfig Config => (FileSyncerConfig)_config;

    public FileSyncer(FileSyncerConfig config) : base(config) { }

    public override async Task<T?> FullLoad<T>(T default_value) where T : default {
        if (Config.DeleteExisting && File.Exists(Config.FilePath)) File.Delete(Config.FilePath);
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

    public override Task<BinaryDiff> Pull(BinaryDiff diff) => 
        throw new NotImplementedException("Active file watching is not implemented.");

    public override async Task Push(BinaryDiff diff) {
        await diff.ApplyToAsync(Config.FilePath);
    }

    public override async Task ClosingPush(BinaryDiff diff) => 
        await Push(diff);
}