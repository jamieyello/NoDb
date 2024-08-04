using System.Collections;
using System;

namespace SlothSerializer.Internal;

// Note: All error checking code will be commented out and kept in comment form
// when all errors are ruled out. This is internal time-sensitive code.

// All commented code should be maintained regardless.

// removing hash functionality entirely from this


internal class StorageBlock<T> : IList<T> {
    public const int DEFAULT_BLOCK_SIZE = 256;
    readonly T[] _block;
    readonly int _block_size;
    //ulong _hash;
    //bool _needs_hash_update = true;
    //readonly bool _cast_to_ulong;

    int _start;
    public int Count {get; private set; }
    public bool IsReadOnly => false;
    public bool IsFull => Count == _block_size;

    public T this[int index] {
        get {
            if (index >= Count) throw new IndexOutOfRangeException();
            return _block[(_start + index) % _block_size];
        } 
        set {
            if (index >= Count) throw new IndexOutOfRangeException();
            _block[(_start + index) % _block_size] = value;
        }
    }

    public StorageBlock() {
        //_cast_to_ulong = typeof(ulong).IsAssignableFrom(typeof(T));
        _block_size = DEFAULT_BLOCK_SIZE;
        _block = new T[_block_size];
    }

    public StorageBlock(int size = DEFAULT_BLOCK_SIZE) {
        //_cast_to_ulong = typeof(ulong).IsAssignableFrom(typeof(T));
        _block_size = size;
        _block = new T[_block_size];
    }

    // public ulong GetBlockHash() {
    //     if (!_needs_hash_update) return _hash;
    //     _hash = 
    //         _cast_to_ulong ? KnuthHash.Calculate(_block[0..Count].Cast<ulong>()) :
    //         KnuthHash.Calculate(_block[0..Count].Select(x => x?.GetHashCode() ?? 0));
    //     return _hash;
    // }

    // not tested
    public void Add(T item) {
        if (Count == _block_size) throw new Exception("Block already at max capacity.");
        _block[Count++ % _block_size] = item;
    }

    // not done
    public void Prepend() {
        throw new NotImplementedException();
    }

    // not tested
    public int IndexOf(T item) {
        for (int i = 0; i < Count; i++) {
            if ((object?)_block[i % _block_size] == (object?)item) return i;
        }
        return -1;
    }

    // not tested
    // optimize: copy lower half downwards if faster
    public void Insert(int index, T item) {
        if (Count == _block_size) throw new Exception("Block already at max capacity.");

        var span = new Span<T>(_block); 
        for (int i = _start + Count; i > _start + index; i--) {
            span[MinMaxMod(i, _block_size)] = span[MinMaxMod(i - 1, _block_size)];
        }
        span[_start + index] = item;
        Count++;
    }

    // not tested
    // optimize: copy lower half downwards if faster
    public void Insert(int index, T[] values) {
        if (Count + values.Length > _block_size) throw new Exception("No room for given values.");
        var block = new Span<T>(_block); 
        for (int i = _start + Count; i > _start + index + values.Length; i--) {
            block[MinMaxMod(i, _block_size)] = block[MinMaxMod(i - values.Length, _block_size)];
        }
        for (int i = _start + index; i < _start + index + values.Length; i++) {
            block[i % _block_size] = values[i - (_start + index)];
        }
        Count += values.Length;
    }

    // not done
    public void RemoveAt(int index)
    {
        var b_span = new Span<T>(_block);

        Count--;
    }

    // not done
    public void RemoveRange(int start, int count) {
        throw new NotImplementedException();
    }

    // done
    public void Clear() {
        _start = 0;
        Count = 0;
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

    public float GetMemoryEfficiency() => 
        (float)Count / _block_size;

    static int MinMaxMod(int value, int mod) {
        while (value < 0) value += mod;
        return value % mod;
    }

    static void ArrayCopyMod() {

    }
}