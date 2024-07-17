using NoDb.Syncers;
using SlothSerializer;

namespace NoDb.Tests.Syncers;

public class DebugSyncerConfig<T> : SyncerConfig
{
    public override bool Load { get; set; } = false;
    public override bool SupportsPull => false;

    public object? DefaultValue;
    public SerializeMode SerializeMode { get; set; } = SerializeMode.Properties;

    public DebugSyncerConfig() { }
    public DebugSyncerConfig(T default_value) => 
        DefaultValue = default_value;

    protected override Syncer GetSyncer() => 
        new DebugSyncer<T>(this);
}