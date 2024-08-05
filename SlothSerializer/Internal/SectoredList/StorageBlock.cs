using System.Collections;

namespace SlothSerializer.Internal;

// Note: All error checking code will be commented out and kept in comment form
// when all errors are ruled out. This is internal time-sensitive code. All 
// commented code should be maintained regardless.

/// <summary> A fixed size list that allocates a looping array of memory. </summary>
internal class StorageBlock<T> : IList<T> {
    public const int DEFAULT_BLOCK_SIZE = 256;
    readonly T[] _block;
    readonly int _block_size;
    int _start;
    public int Count {get; private set; }
    public bool IsReadOnly => false;
    public bool IsFull => Count == _block_size;
    public int FreeSpace => _block_size - Count;

    public StorageBlock<T>? NextBlock { get; set; }
    public StorageBlock<T>? PreviousBlock { get; set; }

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
        _block_size = DEFAULT_BLOCK_SIZE;
        _block = new T[_block_size];
    }

    public StorageBlock(int size = DEFAULT_BLOCK_SIZE) {
        _block_size = size;
        _block = new T[_block_size];
    }

    // not tested
    public void Add(T item) {
        if (Count == _block_size) throw new Exception("Block already at max capacity.");
        _block[Count++ % _block_size] = item;
    }

    // add slower overloads
    // not tested
    public void AddRange(ReadOnlySpan<T> items) {
        if (Count + items.Length > _block_size) throw new Exception("Block already at max capacity.");
        for (int i = 0; i < items.Length; i++) {
            _block[Count + i % _block_size] = items[i];
        }
        Count += items.Length;
    }

    // not tested
    public void Prepend(T item) {
        if (Count == _block_size) throw new Exception("Block already at max capacity.");
        _start--;
        _block[MinMaxMod(ref _start, _block_size)] = item;
    }

    // not tested
    public int IndexOf(T item) {
        for (int i = _start; i < Count; i++) {
            if ((object?)_block[i % _block_size] == (object?)item) return i;
        }
        return -1;
    }

    // not tested
    // optimize: copy lower half downwards if faster
    public void Insert(int index, T item) {
        if (Count == _block_size) throw new Exception("Block already at max capacity.");
        if (index >= Count) throw new IndexOutOfRangeException("Index out of range.");

        var span = new Span<T>(_block); 
        for (int i = _start + index + Count; i > _start + index; i--) {
            span[MinMaxMod(i, _block_size)] = span[MinMaxMod(i - 1, _block_size)];
        }
        span[_start + index] = item;
        Count++;
    }

    // not tested
    // optimize: copy lower half downwards if faster
    public void Insert(int index, ReadOnlySpan<T> values) {
        if (Count + values.Length > _block_size) throw new Exception("No room for given values.");
        if (index > Count) throw new IndexOutOfRangeException("Index out of range.");
        var block = new Span<T>(_block); 
        for (int i = _start + index + Count + values.Length; i > _start + index; i--) {
            block[MinMaxMod(i, _block_size)] = block[MinMaxMod(i - values.Length, _block_size)];
        }
        for (int i = _start + index; i < _start + index + values.Length; i++) {
            block[i % _block_size] = values[i - (_start + index)];
        }
        Count += values.Length;
    }

    // not tested
    public void RemoveAt(int index) {
        if (index >= Count || index < 0) throw new IndexOutOfRangeException("Index out of range.");

        var span = new Span<T>(_block); 
        for (int i = _start + index; i < _start + index + Count; i++) {
            span[i % _block_size] = span[(i + 1) % _block_size];
        }
        Count--;
    }

    // not tested
    public void RemoveRange(int index, int length) {
        if (index >= Count || index < 0 || index + length > Count) throw new IndexOutOfRangeException("Index out of range.");

        var span = new Span<T>(_block); 
        for (int i = _start + index; i < _start + index + Count - length; i++) {
            span[i % _block_size] = span[(i + length) % _block_size];
        }
        Count -= length;
    }

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

    public double GetMemoryEfficiency() => 
        (double)Count / _block_size;

    static int MinMaxMod(int value, int mod) {
        while (value < 0) value += mod;
        return value % mod;
    }

    static int MinMaxMod(ref int value, int mod) {
        while (value < 0) value += mod;
        value %= mod;
        return value;
    }
}