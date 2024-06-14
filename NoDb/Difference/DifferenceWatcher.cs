using SlothSerializer;

namespace NoDb.Difference;

/// <summary>
/// Watches an object at regular intervals and triggers a method when it changes. 
/// Uses serialization to check for differences, does not use any other kind of comparer.
/// </summary>
public class DifferenceWatcher<T> where T : class
{
    readonly T obj;
    readonly DifferenceWatcherOptions _diff_options;
    readonly System.Timers.Timer? _timer;
    readonly object _value_lock = new();
    readonly object _update_lock = new();
    readonly EventHandler<DifferenceWatcherEventArgs<T>> _sync_update;
    BitBuilderBuffer _previous_value;
    bool _initial = true;

    public T Value {
        get {
            lock (_value_lock) {
                return obj;
            }
        }
    }

    public DifferenceWatcher(T obj, Func<DifferenceWatcherEventArgs<T>, Task> update_event_callback, DifferenceWatcherOptions? diff_options = null) {
        this.obj = obj;
        _previous_value = new();
        _previous_value.Append(obj);
        _diff_options = diff_options ?? new();
        _sync_update += (o, e) => update_event_callback(e);
        if (_diff_options.SyncInterval.HasValue) {
            _timer = new(_diff_options.SyncInterval.Value);
            _timer.Elapsed += (o, e) => CheckForUpdate();
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }
    }

    public void CheckForUpdate() {
        lock (_update_lock) { // This needs to not queue every call, will lead to bad things
            try {
                Monitor.Enter(_value_lock);
                var current = new BitBuilderBuffer();
                current.Append(obj);

                if (!current.Matches(_previous_value) || (_initial && _diff_options.TriggerInitial)) {
                    _sync_update.Invoke(obj, new() { Value = obj, Diff = new(_previous_value.GetReader(), current.GetReader(), _diff_options.DiffMethod) });
                    _previous_value?.Clear();
                    _previous_value = current;
                    _initial = false;
                }
            }
            catch (Exception) {
                throw;
            }
            finally {
                //Monitor.Exit(_update_lock);
                Monitor.Exit(_value_lock);
            }
        }
    }
}
