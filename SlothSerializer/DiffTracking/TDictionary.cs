using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace SlothSerializer.DiffTracking;

/// <summary> A tracked Dictionary. </summary>
public class TDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull {
    readonly List<ChangeLogIndex<TKey, TValue>> _change_log = new();
    readonly Dictionary<TKey, TValue> _dictionary = new();
    readonly bool _is_primative;

    public TValue this[TKey key] { 
        get => _dictionary[key];
        set {
            if (_is_primative && _dictionary.ContainsKey(key)) return;
            _change_log.Add(new(key, ChangeActionType.Set, value));
            _dictionary[key] = value;
        }
    }

    public ICollection<TKey> Keys => 
        _dictionary.Keys;

    public ICollection<TValue> Values => 
        _dictionary.Values;

    public int Count => 
        _dictionary.Count;

    public bool IsReadOnly => 
        false;

    public TDictionary() =>
        _is_primative = typeof(TKey).IsPrimitive || (typeof(TKey) == typeof(string));

    public TDictionary(IDictionary<TKey, TValue> dictionary) : this() {
        foreach (var kvp in dictionary) _dictionary.Add(kvp.Key, kvp.Value);
    }

    public void Add(TKey key, TValue value) {
        _change_log.Add(new(key, ChangeActionType.Add, value));
        _dictionary.Add(key, value);
    }

    public void Add(KeyValuePair<TKey, TValue> item) {
        _change_log.Add(new(item.Key, ChangeActionType.Add, item.Value));
        _dictionary.Add(item.Key, item.Value);
    }

    public void Clear() {
        _change_log.Add(new(default, ChangeActionType.Clear));
        _dictionary.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) =>
        _dictionary.Contains(item);

    public bool ContainsKey(TKey key) =>
        _dictionary.ContainsKey(key);
    
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
        ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
        _dictionary.GetEnumerator();

    public bool Remove(TKey key) {
        _change_log.Add(new(key, ChangeActionType.Remove));
        return _dictionary.Remove(key);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) =>
        Remove(item.Key);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) =>
        _dictionary.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() =>
        _dictionary.GetEnumerator();
}