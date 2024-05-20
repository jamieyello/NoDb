using SlothSerializer;

namespace NoDb.Difference;

/// <summary>
/// Watches an object at regular intervals and triggers a method when it changes. 
/// Uses serialization to check for differences, does not use any other kind of comparer.
/// </summary>
public class DifferenceWatcher<T> where T : class
{
    readonly T obj;
    readonly DifferenceWatcherOptions options;
    readonly System.Timers.Timer? _timer;
    readonly object _value_lock = new();
    readonly object _update_lock = new();
    readonly EventHandler<DifferenceWatcherEventArgs<T>> _sync_update;
    BitBuilderBuffer? _previous_value;

    public T Value {
        get {
            lock (_value_lock) {
                return obj;
            }
        }
    }

    public DifferenceWatcher(T obj, Func<DifferenceWatcherEventArgs<T>, Task> update_event_callback, DifferenceWatcherOptions? options = null) {
        this.obj = obj;
        this.options = options ?? new();
        _sync_update += (o, e) => update_event_callback(e);
        if (this.options.SyncInterval.HasValue) {
            _timer = new(this.options.SyncInterval.Value);
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

                // debugging breakpoint --
                if (_previous_value != null) { 
                    bool match = current.Matches(_previous_value);
                }

                if (_previous_value == null || !current.Matches(_previous_value)) {
                    if (options.TriggerInitial || _previous_value != null) {
                        _sync_update.Invoke(obj, new() { Value = obj, Reader = current.GetReader() });
                    }
                    _previous_value?.Clear();
                    _previous_value = current;
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
