using SlothSerializer;
using SlothSerializer.Internal;

namespace NoDb.Syncers;

public abstract class Syncer {
    protected readonly SyncerConfig _config;
    public bool IsClosed { get; protected set; }

    protected Syncer(SyncerConfig config) {
        _config = config;
    }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public virtual async Task<BitBuilderBuffer> FullLoad() => throw new NotImplementedException();
    public virtual async Task<bool> Push(BinaryDiff diff) => throw new NotImplementedException();
    public virtual async Task<BinaryDiff> Pull(BinaryDiff diff) => throw new NotImplementedException();
    public virtual async Task ClosingPush(BinaryDiff diff) => throw new NotImplementedException();
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
