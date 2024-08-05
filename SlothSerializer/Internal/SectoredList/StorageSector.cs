using System.Collections;

namespace SlothSerializer.Internal;

internal class StorageSector<T> : IList<T> {
    readonly List<StorageBlock<T>> _storage;
    readonly int _sector_size = StorageBlock<T>.DEFAULT_BLOCK_SIZE;
    StorageBlock<T> _start_block;
    StorageBlock<T> _end_block;
    int _start_index;
    int _end_index;

    ulong _hash;
    bool _needs_hash_update = true;
    readonly bool _cast_to_ulong;

    public int Count { get; private set; }
    public bool IsReadOnly => false;

    public T this[int index] { 
        get => throw new NotImplementedException(); 
        set => throw new NotImplementedException(); 
    }

    public StorageSector(
        List<StorageBlock<T>> storage, 
        StorageBlock<T> start_block,
        StorageBlock<T> end_block,
        int start_index,
        int end_index)
    {
        _cast_to_ulong = typeof(ulong).IsAssignableFrom(typeof(T));
        _storage = storage;
        _start_block = start_block;
        _end_block = end_block;
        _start_index = start_index;
        _end_index = end_index;
    }

    public int IndexOf(T item)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, T item)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    public void Add(T item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(T item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public ulong GetSectorHash() {
        if (!_needs_hash_update) return _hash;
        _hash = 
            _cast_to_ulong ? KnuthHash.Calculate(this.Cast<ulong>()) :
            KnuthHash.Calculate(this.Select(x => x?.GetHashCode() ?? 0));
        _needs_hash_update = false;
        return _hash;
    }
}