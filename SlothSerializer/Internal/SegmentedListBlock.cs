using System.Collections;

namespace SlothSerializer.Internal;

// Note: All error checking code will be commented out and kept in comment form
// when all errors are ruled out. This is internal time-sensitive code.

// All commented code should be maintained regardless.

internal class SegmentedListBlock<T> : IList<T> {
    public const int DEFAULT_BLOCK_SIZE = 256;
    readonly T[] _block;
    /// <summary> The length of each individual array. </summary>
    readonly int _block_size;
    ulong _hash;
    bool _needs_hash_update = true;
    readonly bool _cast_to_ulong;

    public int Count {get; private set; }
    public bool IsReadOnly => false;
    public bool IsFull => Count == _block_size;

    public T this[int index] {
        get {
            if (index >= Count) throw new IndexOutOfRangeException();
            return _block[index];
        } 
        set {
            if (index >= Count) throw new IndexOutOfRangeException();
            _block[index] = value;
            _needs_hash_update = true;
        }
    }

    public SegmentedListBlock() {
        _cast_to_ulong = typeof(ulong).IsAssignableFrom(typeof(T));
        _block_size = DEFAULT_BLOCK_SIZE;
        _block = new T[_block_size];
    }

    public SegmentedListBlock(int size = DEFAULT_BLOCK_SIZE) {
        _cast_to_ulong = typeof(ulong).IsAssignableFrom(typeof(T));
        _block_size = size;
        _block = new T[_block_size];
    }

    public ulong GetBlockHash() {
        if (!_needs_hash_update) return _hash;
        _hash = 
            _cast_to_ulong ? KnuthHash.Calculate(_block[0..Count].Cast<ulong>()) :
            KnuthHash.Calculate(_block[0..Count].Select(x => x?.GetHashCode() ?? 0));
        return _hash;
    }

    public void Add(T item) {
        if (Count == _block_size) throw new Exception("Block already at max capacity.");
        _block[Count++] = item;
        _needs_hash_update = true;
    }

    public int IndexOf(T item) {
        for (int i = 0; i < Count; i++) {
            if ((object?)_block[i] == (object?)item) return i;
        }
        return -1;
    }

    public void Insert(int index, T item)
    {
        if (Count >= _block_size - 1) throw new Exception("Block already at max capacity.");
        _needs_hash_update = true;
        Count++;
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        _needs_hash_update = true;
        Count--;
        throw new NotImplementedException();
    }

    public void Clear() {
        Count = 0;
        _needs_hash_update = true;
    }

    public bool Contains(T item) =>
        IndexOf(item) != -1;

    public void CopyTo(T[] array, int array_index) =>
        _block.CopyTo(array, array_index);

    public bool Remove(T item) {
        var index = IndexOf(item);
        if (index == -1) return false;
        RemoveAt(index);
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() => 
        _block[0..Count].GetEnumerator();

    public IEnumerator<T> GetEnumerator() =>
        ((IEnumerable<T>)_block[0..Count]).GetEnumerator();
}