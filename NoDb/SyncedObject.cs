using NoDb.Difference;
using NoDb.Syncers;

namespace NoDb;

public class SyncedObject<T> where T : class
{
    readonly List<Syncer> _syncers = new();
    readonly DifferenceWatcher<T> _watcher;

    public T Value { get; set; }

    public SyncedObject(T obj, SyncerConfig config, DifferenceWatcherOptions? watcher_options = null)
    {
        _watcher = new DifferenceWatcher<T>(obj, OnDifference, watcher_options);
        Value = obj;
    }

    async Task OnDifference(DifferenceWatcherEventArgs<T> args)
    {
        throw new NotImplementedException();
        //await Task.WhenAll(_syncers.Select(x => x.Push()));
    }
}
