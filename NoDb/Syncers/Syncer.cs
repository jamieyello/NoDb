using NoDb.Networking;
using SlothSerializer;

namespace NoDb.Syncers;

public abstract class Syncer {
    protected readonly SyncerConfig _config;
    public bool IsClosed { get; protected set; }
    internal bool Load => _config.Load;

    protected Syncer(SyncerConfig config) {
        _config = config;
    }

    // Due to async overload weirdness these are virtual. All should be overridden anyway.
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public virtual async Task<ConnectionResult> Connect() {
        return ConnectionResult.Success;
    }
    public virtual async Task<T?> FullLoad<T>(T default_value) => throw new NotImplementedException();
    public virtual async Task Push(BitBuilderDiff diff) => throw new NotImplementedException();
    public virtual async Task<BitBuilderDiff> Pull(BitBuilderDiff diff) => throw new NotImplementedException();
    public virtual async Task ClosingPush(BitBuilderDiff diff) => throw new NotImplementedException();
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
