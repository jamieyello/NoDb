using NoDb.Difference;
using NoDb.Syncers;

namespace NoDb;

// loading: implemented, not tested
// pushing: implemented, not tested
// pulling: not tested
// closing: not implemented
public class SyncedObject<T>
{
    readonly List<Syncer> _syncers = new();
#pragma warning disable IDE0052 // Remove unread private members
    readonly DifferenceWatcher<SyncedObjectContainer<T>> _push_watcher;
#pragma warning restore IDE0052 // Remove unread private members

    readonly SyncedObjectContainer<T> _container;
    public T? Value { 
        get {
            initialized_task.Wait();
            return _container.Value;
        }
        set => _container.Value = value;
    }
    readonly Task initialized_task;

    public SyncedObject(SyncerConfig config, T? default_value = default, DifferenceWatcherConfig? auto_save_options = null) {
        _container = new(default_value);
        _syncers.AddRange(config.GetSyncers());
        _push_watcher = new DifferenceWatcher<SyncedObjectContainer<T>>(_container, OnPushDifference, auto_save_options ?? new());

        var loader = _syncers.Where(x => x.Load).FirstOrDefault();
        initialized_task = loader != null ? FullLoad(loader) : Task.CompletedTask;
    }

    public void WaitForLoad() => initialized_task.Wait();
    public Task WaitForLoadAsync() => initialized_task;
    public void Save() => _push_watcher.CheckForUpdate();
    
    async Task FullLoad(Syncer s) {
        _container.Value = await s.FullLoad(_container.Value);
    }

    public async Task TestingForceFullLoad() {
        var loader = _syncers.Where(x => x.Load).FirstOrDefault();
        await (loader != null ? FullLoad(loader) : Task.CompletedTask);
    }

    void OnPushDifference(DifferenceWatcherEventArgs<SyncedObjectContainer<T>> args) {
        var tasks = Task.WhenAll(_syncers.Select(x => x.Push(args.Diff)));
        tasks.Wait();
    }
}
