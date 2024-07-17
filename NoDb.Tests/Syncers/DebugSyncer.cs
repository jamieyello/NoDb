using NoDb.Syncers;
using SlothSerializer;
using SlothSerializer.Internal;

namespace NoDb.Tests.Syncers;

/// <summary> Allows insight to sync actions/results via debug breakpoints. </summary>
public class DebugSyncer<DebugT> : Syncer
{
    readonly byte[] bb_array;

    DebugSyncerConfig<DebugT> DebugSyncerConfig => (DebugSyncerConfig<DebugT>)_config;

    public DebugSyncer(DebugSyncerConfig<DebugT> config) : base(config)
    {
        bb_array = ToBitBuilderArray(config.DefaultValue, config.SerializeMode);
    }

    public override Task<T?> FullLoad<T>(T default_value) where T : default
    {
        return Task.Run(() => {
            var value = FromBitBuilderArray<T>(bb_array, DebugSyncerConfig.SerializeMode);
            return value;
        });
    }

    public override Task<BinaryDiff> Pull(BinaryDiff diff)
    {
        return base.Pull(diff);
    }

    public override async Task Push(BinaryDiff diff)
    {
        using var ms = new MemoryStream(bb_array);
        var before = FromBitBuilderArray<DebugT>(bb_array, DebugSyncerConfig.SerializeMode);
        await diff.ApplyToAsync(ms);
        var after = FromBitBuilderArray<DebugT>(bb_array, DebugSyncerConfig.SerializeMode);
    }

    public override Task ClosingPush(BinaryDiff diff) => 
        Push(diff);
    
    static byte[] ToBitBuilderArray(object? value, SerializeMode mode) {
        var bb = new BitBuilderBuffer();
        bb.Append(value, mode);
        return bb.EnumerateAsBytes().ToArray();
    }

    static T? FromBitBuilderArray<T>(byte[] array, SerializeMode mode) {
        using var ms = new MemoryStream(array);
        var bb = new BitBuilderBuffer();
        bb.ReadFromStream(ms);
        return bb.GetReader().Read<T>(mode);
    }
}