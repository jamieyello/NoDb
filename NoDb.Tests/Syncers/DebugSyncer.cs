using System.Diagnostics;
using NoDb.Syncers;
using SlothSerializer;
using SlothSerializer.Internal;

namespace NoDb.Tests.Syncers;

/// <summary> Allows insight to sync actions/results via debug breakpoints. </summary>
public class DebugSyncer<DebugT> : Syncer
{
    int full_load_count = 0;
    int push_count = 0;
    readonly byte[] bb_array;

    public DebugSyncer(DebugSyncerConfig<DebugT> config) : base(config) {
        bb_array = ToBitBuilderArray(config.DefaultValue, config.SerializeMode);
    }

    DebugSyncerConfig<DebugT> DebugSyncerConfig => (DebugSyncerConfig<DebugT>)_config;

    public override Task<T?> FullLoad<T>(T default_value) where T : default {
        Inspect($"Full load #{++full_load_count} inspection.");
        return Task.Run(() => {
            var value = FromBitBuilderArray<T>(bb_array, DebugSyncerConfig.SerializeMode);
            return value;
        });
    }

    public override Task<BinaryDiff> Pull(BinaryDiff diff) {
        return base.Pull(diff);
    }

    public override async Task Push(BinaryDiff diff) {
        Inspect($"Pre-push #{++push_count} inspection.");
        using var ms = new MemoryStream(bb_array);
        await diff.ApplyToAsync(ms);
        Inspect($"Post-push #{push_count} inspection.");
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

    void Inspect(string message) {
        if (DebugSyncerConfig.InspectMethod == null) return;
        using var ms = new MemoryStream(bb_array);
        var bb = new BitBuilderBuffer();
        bb.ReadFromStream(ms);
        DebugSyncerConfig.InspectMethod(message, bb);
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}