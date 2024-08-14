using NoDb.Syncers;
using SlothSerializer;
using SlothSerializer.DiffTracking;

namespace NoDb.Tests.Syncers;

/// <summary> Allows insight to sync actions/results via debug breakpoints. </summary>
public class DebugSyncer<DebugT> : Syncer
{
    int full_load_count = 0;
    int push_count = 0;
    readonly MemoryStream bb_ms = new();
    public SyncedObject<DebugT> InspectSyncer { get; set; }

    public DebugSyncer(DebugSyncerConfig<DebugT> config) : base(config) {
        ToBitBuilderStream(config.DefaultValue, bb_ms, config.SerializeMode);
    }

    DebugSyncerConfig<DebugT> DebugSyncerConfig => (DebugSyncerConfig<DebugT>)_config;

    public override Task<T?> FullLoad<T>(T default_value) where T : default {
        Inspect($"Full load #{++full_load_count} inspection.");
        return Task.Run(() => {
            var value = FromBitBuilderStream<T>(bb_ms, DebugSyncerConfig.SerializeMode);
            return value;
        });
    }

    public override Task<BitBuilderDiff> Pull(BitBuilderDiff diff) {
        return base.Pull(diff);
    }

    public override async Task Push(BitBuilderDiff diff) {
        Inspect($"Pre-push #{++push_count} inspection.");
        await diff.ApplyToAsync(bb_ms);
        Inspect($"Post-push #{push_count} inspection.");
    }

    public override Task ClosingPush(BitBuilderDiff diff) => 
        Push(diff);
    
    static void ToBitBuilderStream(object? value, MemoryStream stream, SerializeMode mode) {
        var bb = new BitBuilderBuffer();
        bb.Append(value, mode);
        stream.Position = 0;
        stream.SetLength(0);
        bb.WriteToStream(stream);
        stream.Position = 0;
    }

    static T? FromBitBuilderStream<T>(MemoryStream stream, SerializeMode mode) {
        var bb = new BitBuilderBuffer();
        stream.Position = 0;
        bb.ReadFromStream(stream);
        stream.Position = 0;
        return bb.GetReader().Read<T>(mode);
    }

    void Inspect(string message) {
        if (DebugSyncerConfig.InspectMethod == null) return;
        var bb = new BitBuilderBuffer();
        bb_ms.Position = 0;
        bb.ReadFromStream(bb_ms);
        bb_ms.Position = 0;
        DebugSyncerConfig.InspectMethod(message, bb);
    }
}