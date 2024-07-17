using NoDb.Syncers;
using SlothSerializer;
using SlothSerializer.Internal;

namespace NoDb.Tests.Syncers;

public class DebugSyncer : Syncer
{
    BitBuilderBuffer value_data;

    public DebugSyncer(DebugSyncerConfig config) : base(config)
    {
        value_data = new();
        value_data.Append(config.DefaultValue);
    }

    public override Task<T?> FullLoad<T>(T default_value) where T : default
    {
        return Task.Run(() => value_data.GetReader().Read<T>());
    }

    public override Task<BinaryDiff> Pull(BinaryDiff diff)
    {
        return base.Pull(diff);
    }

    public override Task Push(BinaryDiff diff)
    {
        return Task.Run(() => diff.ApplyToAsync());
    }

    public override Task ClosingPush(BinaryDiff diff)
    {
        return base.ClosingPush(diff);
    }
}