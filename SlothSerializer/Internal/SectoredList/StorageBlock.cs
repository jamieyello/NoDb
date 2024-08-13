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

/// <summary> A fixed size list that allocates a looping array of memory. </summary>
// internal class StorageBlock<T> : IList<T> {
//     public const int DEFAULT_BLOCK_SIZE = 256;
//     readonly T[] _block;
//     readonly int _block_size;
//     int _start;
//     public int Count {get; private set; }
//     public bool IsReadOnly => false;
//     public bool IsFull => Count == _block_size;
//     public int FreeSpace => _block_size - Count;

//     public StorageBlock<T>? NextBlock { get; set; }
//     public StorageBlock<T>? PreviousBlock { get; set; }

//     public T this[int index] {
//         get {
//             if (index >= Count) throw new IndexOutOfRangeException();
//             return _block[(_start + index) % _block_size];
//         } 
//         set {
//             if (index >= Count) throw new IndexOutOfRangeException();
//             _block[(_start + index) % _block_size] = value;
//         }
//     }

//     public StorageBlock() {
//         _block_size = DEFAULT_BLOCK_SIZE;
//         var l = new List<int>();
//         l.Ins()

//         _block = new T[_block_size];
//     }

//     public StorageBlock(int size) {
//         _block_size = size;
//         _block = new T[_block_size];
//     }

//     // not tested
//     public void Add(T item) {
//         if (Count == _block_size) throw new Exception("Block already at max capacity.");
//         _block[Count++ % _block_size] = item;
//     }

//     // add slower overloads
//     // not tested
//     public void AddRange(ReadOnlySpan<T> items) {
//         if (Count + items.Length > _block_size) throw new Exception("Block already at max capacity.");
//         for (int i = 0; i < items.Length; i++) {
//             _block[Count + i % _block_size] = items[i];
//         }
//         Count += items.Length;
//     }

//     // not tested
//     public void Prepend(T item) {
//         if (Count == _block_size) throw new Exception("Block already at max capacity.");
//         _start--;
//         _block[MinMaxMod(ref _start, _block_size)] = item;
//     }

//     // not tested
//     public int IndexOf(T item) {
//         for (int i = _start; i < Count; i++) {
//             if ((object?)_block[i % _block_size] == (object?)item) return i;
//         }
//         return -1;
//     }

//     // not tested
//     // optimize: copy lower half downwards if faster
//     public void Insert(int index, T item) {
//         if (Count == _block_size) throw new Exception("Block already at max capacity.");
//         if (index >= Count) throw new IndexOutOfRangeException("Index out of range.");

//         var span = new Span<T>(_block); 
//         for (int i = _start + index + Count; i > _start + index; i--) {
//             span[MinMaxMod(i, _block_size)] = span[MinMaxMod(i - 1, _block_size)];
//         }
//         span[_start + index] = item;
//         Count++;
//     }

//     // not tested
//     // optimize: copy lower half downwards if faster
//     public void Insert(int index, ReadOnlySpan<T> values) {
//         if (Count + values.Length > _block_size) throw new Exception("No room for given values.");
//         if (index > Count) throw new IndexOutOfRangeException("Index out of range.");
//         var block = new Span<T>(_block); 
//         for (int i = _start + index + Count + values.Length; i > _start + index; i--) {
//             block[MinMaxMod(i, _block_size)] = block[MinMaxMod(i - values.Length, _block_size)];
//         }
//         for (int i = _start + index; i < _start + index + values.Length; i++) {
//             block[i % _block_size] = values[i - (_start + index)];
//         }
//         Count += values.Length;
//     }

//     // not tested
//     public void RemoveAt(int index) {
//         if (index >= Count || index < 0) throw new IndexOutOfRangeException("Index out of range.");

//         var span = new Span<T>(_block); 
//         for (int i = _start + index; i < _start + index + Count; i++) {
//             span[i % _block_size] = span[(i + 1) % _block_size];
//         }
//         Count--;
//     }

//     // not tested
//     public void RemoveRange(int index, int length) {
//         if (index >= Count || index < 0 || index + length > Count) throw new IndexOutOfRangeException("Index out of range.");

//         var span = new Span<T>(_block); 
//         for (int i = _start + index; i < _start + index + Count - length; i++) {
//             span[i % _block_size] = span[(i + length) % _block_size];
//         }
//         Count -= length;
//     }

//     public void Clear() {
//         _start = 0;
//         Count = 0;
//     }

//     public bool Contains(T item) =>
//         IndexOf(item) != -1;

//     public void CopyTo(T[] array, int array_index) =>
//         _block.CopyTo(array, array_index);

//     public bool Remove(T item) {
//         var index = IndexOf(item);
//         if (index == -1) return false;
//         RemoveAt(index);
//         return true;
//     }

//     IEnumerator IEnumerable.GetEnumerator() => 
//         _block[0..Count].GetEnumerator();

//     public IEnumerator<T> GetEnumerator() =>
//         ((IEnumerable<T>)_block[0..Count]).GetEnumerator();

//     public double GetMemoryEfficiency() => 
//         (double)Count / _block_size;

//     static int MinMaxMod(int value, int mod) {
//         while (value < 0) value += mod;
//         return value % mod;
//     }

//     static int MinMaxMod(ref int value, int mod) {
//         while (value < 0) value += mod;
//         value %= mod;
//         return value;
//     }
// }