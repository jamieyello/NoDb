using NoDb.Difference;
using NoDb.Syncers;

namespace NoDb;

// loading: implemented, not tested
// pushing: implemented, not tested
// pulling: not tested
// closing: not implemented
public class SyncedObject<T> : IDisposable
{
    readonly List<Syncer> _syncers = new();
#pragma warning disable IDE0052 // Remove unread private members
    readonly DifferenceWatcher<T> _push_watcher;
#pragma warning restore IDE0052 // Remove unread private members

    readonly SyncedObjectContainer<T> _container;
    readonly Task _initialize_task;
    private bool disposedValue;

    public T? Value {
        get {
            _initialize_task.Wait();
            return _container.Value;
        }
        set => _container.Value = value;
    }

    public SyncedObject(SyncerConfig config, T? default_value = default, DifferenceWatcherConfig? auto_save_options = null) {
        _container = new(default_value);
        _syncers.AddRange(config.GetSyncers());
        _push_watcher = new DifferenceWatcher<T>(_container, OnPushDifference, auto_save_options ?? new());
        _initialize_task = InitializeTask();
    }

    async Task InitializeTask() {
        var connection_tasks = _syncers.Select(x => x.Connect());
        await Task.WhenAll(connection_tasks);

        var loader = _syncers.Where(x => x.Load).FirstOrDefault();
        if (loader != null) await FullLoad(loader);
    }

    public SyncedObject<T> Loaded() {
        _initialize_task.Wait();
        return this;
    }

    public async Task<SyncedObject<T>> LoadedAsync() {
        await _initialize_task;
        return this;
    }

    public T? WaitForLoad() {
        _initialize_task.Wait();
        return Value;
    }

    public async Task<T?> WaitForLoadAsync() {
        await _initialize_task;
        return Value;
    }

    public void Sync() => _push_watcher.CheckForUpdate();
    
    async Task FullLoad(Syncer s) {
        _container.Value = await s.FullLoad(_container.Value);
    }

    public async Task TestingForceFullLoad() {
        var loader = _syncers.Where(x => x.Load).FirstOrDefault();
        await (loader != null ? FullLoad(loader) : Task.CompletedTask);
    }

    void OnPushDifference(DifferenceWatcherEventArgs<T> args) {
        var tasks = Task.WhenAll(_syncers.Select(x => x.Push(args.Diff)));
        tasks.Wait();
    }

    public IEnumerable<Syncer> GetSyncers() => _syncers.AsEnumerable();
    /// <summary> Get all attached syncers of the specified type. </summary>
    public ST[] GetSyncers<ST>() where ST : Syncer => 
        _syncers.Where(x => x.GetType() == typeof(ST)).Select(x => (ST)x).ToArray();

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _push_watcher.Stop();
                _push_watcher.CheckForUpdate();
                _push_watcher.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
