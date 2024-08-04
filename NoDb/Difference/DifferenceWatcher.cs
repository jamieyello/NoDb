using SlothSerializer;

namespace NoDb.Difference;

/// <summary>
/// Watches an object at regular intervals and triggers a method when it changes. 
/// Uses serialization to check for differences, does not use any other kind of comparer.
/// </summary>
internal class DifferenceWatcher<T> : IDisposable
{
    readonly SyncedObjectContainer<T> _container;
    readonly DifferenceWatcherConfig _config;
    readonly System.Timers.Timer? _timer;
    readonly object _value_lock = new();
    readonly object _update_lock = new();
    readonly EventHandler<DifferenceWatcherEventArgs<T>> _sync_update;
    readonly BitBuilderBuffer _previous_value = new();
    readonly BitBuilderBuffer _current_value = new();
    bool _initial = true;
    private bool disposedValue;

    public DifferenceWatcher(SyncedObjectContainer<T> _container, Action<DifferenceWatcherEventArgs<T>> update_event_callback, DifferenceWatcherConfig config) {
        this._container = _container;
        _previous_value.Append(_container.Value);
        _config = config;
        _sync_update += (o, e) => update_event_callback(e);
        if (_config.AutoSyncInterval.HasValue) {
            _timer = new(_config.AutoSyncInterval.Value);
            _timer.Elapsed += (o, e) => CheckForUpdate();
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }
    }

    public void CheckForUpdate() {
        lock (_update_lock) { // This needs to not queue every call, will lead to bad things
            try {
                Monitor.Enter(_current_value);
                _current_value.Clear();
                _current_value.Append(_container.Value);

                if (!_current_value.Matches(_previous_value) || (_initial && _config.TriggerInitial)) {
                    _sync_update.Invoke(this, new() { Value = _container.Value, Diff = new(_previous_value, _current_value, _config.DiffMethod) });
                    _previous_value.Clear();
                    _previous_value.Append(_container);
                    _initial = false;
                }
            }
            catch (Exception) {
                throw;
            }
            finally {
                //Monitor.Exit(_update_lock);
                Monitor.Exit(_current_value);
            }
        }
    }

    public void Stop() => _timer?.Stop();

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _timer?.Dispose();
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
