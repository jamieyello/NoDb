using System.Collections;

namespace SlothSerializer.Internal;

/// <summary>
/// This class brings down the memory usage of List, especially with valuetypes and structs,
/// by stringing small arrays together rather than doubling one big one when the capacity is reached.
/// Also optimized for hashing, caching a hash for each segment. Insert operations are also 
/// heavily optimized over the default C# List.
/// </summary>
/// <remarks>
/// This data structure is optimal for data that needs constant modification and monitoring. Read/Write
/// operations may slightly suffer some speed loss.
/// </remarks>
public class SegmentedList<T> : IList<T> {
    // Base parameters
    /// <summary> The combined arrays. </summary>
    readonly List<StorageBlock<T>> _blocks;

    public int Count { get; private set; }
    public bool IsReadOnly => false;
    readonly int _block_size = StorageBlock<T>.DEFAULT_BLOCK_SIZE;

    public T this[int index] {
        get {
            if (index >= Count) throw new IndexOutOfRangeException();
            return _blocks[index / _block_size][index % _block_size];
        } 
        set {
            if (index >= Count) throw new IndexOutOfRangeException();
            _blocks[index / _block_size][index % _block_size] = value;
        } 
    }

    public SegmentedList() =>
        _blocks = new() { new(_block_size) };

    public SegmentedList(int block_size) {
        _block_size = block_size;
        _blocks = new() { new(_block_size) };
    }

    public SegmentedList(IEnumerable<T> values, int? block_size = null) {
        if (block_size != null) _block_size = block_size.Value;
        _blocks = new() { new(_block_size) };
        foreach (var item in values) Add(item);
    }

    public void Add(T item) {
        if (_blocks[^1].IsFull) {
            _blocks.Add(new(_block_size));
        }
        _blocks[^1].Add(item);
        Count++;
    }

    public void RemoveAt(int index) {
        throw new NotImplementedException();
    }

    public int IndexOf(T item) {
        int i_c = 0;

        if (item == null) {
            foreach (var i in this) {
                if (i == null) return i_c;
                i_c++;
            }
            return -1;
        }
        foreach (var i in this) {
            if (item.Equals(i)) return i_c;
            i_c++;
        }
        return -1;
    }

    public void Insert(int index, T item) {
        throw new NotImplementedException();
    }

    public void Clear() {
        Count = 0;
        _blocks.Clear();
        _blocks.Add(new(_block_size));
    }

    public bool Contains(T item) =>
        IndexOf(item) != -1;

    public void CopyTo(T[] array, int arrayIndex) {
        throw new NotImplementedException();
    }

    public bool Remove(T item) {
        var i = IndexOf(item);
        if (i == -1) return false;
        RemoveAt(i);
        return true;
    }

    public IEnumerator<T> GetEnumerator() {
        int c = 0;
        for (int b = 0; b < _blocks.Count; b++) {
            for (int i = 0; i < _blocks[b].Count; i++) {
                yield return _blocks[b][i];
                if (++c == Count) yield break;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    // public ulong GetHash() => 
    //     KnuthHash.Calculate(_blocks.Select(x => x.GetBlockHash()));
}
