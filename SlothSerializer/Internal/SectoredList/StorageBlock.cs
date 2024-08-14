using System.Collections;

namespace SlothSerializer.Internal;

/// <summary>
/// A fixed size list that keeps a cached hash.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class StorageBlock<T> : IList<T> {
    public const int DEFAULT_BLOCK_SIZE = 256;
    readonly List<T> _block;
    readonly int _block_size;
    public bool IsFull => Count == _block_size;
    public int FreeSpace => _block_size - Count;

    ulong _hash;
    bool _needs_hash_update = true;
    readonly bool _hash_as_ulong;

    public T this[int index] { 
        get => _block[index];
        set => _block[index] = value;
    }

    public T[] this[Range range] {
        get => _block.GetRange(range.Start.Value, range.End.Value - range.Start.Value).ToArray();
    }

    public int Count => _block.Count;
    public bool IsReadOnly => false;

    public StorageBlock() {
        _hash_as_ulong = typeof(ulong).IsAssignableFrom(typeof(T));
        _block_size = DEFAULT_BLOCK_SIZE;
        _block = new(_block_size);
    }

    public StorageBlock(int size) {
        _hash_as_ulong = typeof(ulong).IsAssignableFrom(typeof(T));
        _block_size = size;
        _block = new(_block_size);
    }

    public void Add(T item) {
        if (Count == _block_size) throw new Exception("Block already at max capacity.");
        _block.Add(item);
        _needs_hash_update = true;
    }

    public void AddRange(ICollection<T> items) {
        if (Count + items.Count > _block_size) throw new Exception("Block already at max capacity.");
        _block.AddRange(items);
        _needs_hash_update = true;
    }

    public void Clear() {
        _block.Clear();
        _needs_hash_update = true;
    }

    public bool Contains(T item) =>
        _block.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) {
        _block.CopyTo(array, arrayIndex);
    }
    
    public IEnumerator<T> GetEnumerator() =>
        _block.GetEnumerator();
    
    public int IndexOf(T item) =>
        _block.IndexOf(item);

    public void Insert(int index, T item) {
        _needs_hash_update = true;
        _block.Insert(index, item);
    }

    public void InsertRange(int index, ICollection<T> items) {
        if (Count + items.Count > _block_size) throw new Exception("Block already at max capacity.");
        _block.InsertRange(index, items);
        _needs_hash_update = true;
    }

    public bool Remove(T item) {
        _needs_hash_update = true;
        return _block.Remove(item);
    }

    public void RemoveAt(int index) {
        _needs_hash_update = true;
        _block.RemoveAt(index);
    }

    public void RemoveRange(int index, int count) {
        if (count > 0) _needs_hash_update = true;
        _block.RemoveRange(index, count);
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        _block.GetEnumerator();
    
    public ulong GetBlockHash() {
        if (!_needs_hash_update) return _hash;
        _hash = 
            _hash_as_ulong ? KnuthHash.Calculate(this.Cast<ulong>()) :
            KnuthHash.Calculate(this.Select(x => x?.GetHashCode() ?? 0));
        _needs_hash_update = false;
        return _hash;
    }

    public double GetMemoryEfficiency() => 
        (double)Count / _block_size;
}