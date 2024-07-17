using NoDb.Syncers;

namespace NoDb.Tests.Syncers;

public class DebugSyncerConfig : SyncerConfig
{
    public override bool Load { get; set; } = false;
    public override bool SupportsPull => false;

    public object? DefaultValue;

    public DebugSyncerConfig() { }
    public DebugSyncerConfig(T default_value) => 
        DefaultValue = default_value;

    protected override Syncer GetSyncer() => 
        new DebugSyncer(this);
}