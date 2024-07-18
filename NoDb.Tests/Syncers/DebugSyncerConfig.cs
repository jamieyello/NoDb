using NoDb.Syncers;
using SlothSerializer;

namespace NoDb.Tests.Syncers;

public class DebugSyncerConfig<T> : SyncerConfig
{
    public override bool Load { get; set; } = true;
    public override bool SupportsPull => false;

    public object? DefaultValue;
    public SerializeMode SerializeMode { get; set; } = SerializeMode.Properties;
    /// <summary> A callback for you to inspect. A string will contain a message to tell you what's going on. </summary>
    public Action<string, BitBuilderBuffer>? InspectMethod { get; set; }

    public DebugSyncerConfig() { }
    public DebugSyncerConfig(T default_value, Action<string, BitBuilderBuffer> inspect_method) {
        DefaultValue = default_value;
        InspectMethod = inspect_method;
    }

    protected override Syncer GetSyncer() => 
        new DebugSyncer<T>(this);
}