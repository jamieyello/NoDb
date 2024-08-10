using System.Collections;

namespace SlothSerializer.Internal;

/// <summary>
/// This class brings down the memory usage of List, especially with valuetypes and structs,
/// by stringing small arrays together rather than doubling one big one when the capacity is reached.
/// Also optimized for hashing, caching a hash for each segment. Insert/remove operations are also 
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
            var bi = MapBlockIndex(index);
            return _blocks[bi.Block][bi.Index];
        } 
        set {
            var bi = MapBlockIndex(index);
            _blocks[bi.Block][bi.Index] = value;
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
        if (_blocks[^1].IsFull) _blocks.Add(new(_block_size));
        _blocks[^1].Add(item);
        Count++;
    }

    public void AddRange(T[] items) {
        int added = 0;
        do {
            if (_blocks[^1].IsFull) _blocks.Add(new(_block_size));
            var add_count = Math.Min(_blocks[^1].FreeSpace, items.Length - added);
            _blocks[^1].AddRange(items[added..(added + add_count)]);
            added += add_count;

        } while (added < items.Length);
        Count += added;
    }

    public void RemoveAt(int index) {
        var bi = MapBlockIndex(index);
        _blocks[bi.Block].RemoveAt(bi.Index);
        Count--;
    }

    public void RemoveRange(int index, int count) {
        var bi = MapBlockIndex(index);
        var remove_index = bi.Index;
        var remove_block = bi.Block;

        var removed = 0;

        // remove start
        if (remove_index > 0) {
            _blocks[remove_block].RemoveRange(remove_index, _blocks[remove_block].Count - remove_index);
            removed += remove_index;
            remove_index = 0;
            remove_block++;
        }

        // remove uninterrupted blocks
        while (_blocks[remove_block].Count <= count - removed) {
            removed += _blocks[remove_block].Count;
            _blocks.RemoveAt(remove_block);
        }

        // remove trailing
        var remainder = count - removed;
        if (remainder > 0) {
            _blocks[remove_block].RemoveRange(0, remainder);
            removed += remainder;
        }
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
        if (_blocks[index].IsFull) {
            _blocks.Add(new(_block_size));
            _blocks[index + 1].Add(item);
        }
        else {
            _blocks[index].Add(item);
        }
        Count++;
    }

    public void InsertRange(int index, T[] items) {
        var bi = MapBlockIndex(index);
        var insert_index = bi.Index;
        var insert_block = bi.Block;

        var inserted = 0;

        // initial insert
        // wrong, needs to insert
        // if (_blocks[insert_block].FreeSpace > 0) {
        //     var insert_count = Math.Min(items.Length, _blocks[insert_block].FreeSpace);
        //     _blocks[insert_block].InsertRange(insert_index, items[..insert_count]);
        //     inserted += insert_count;
        //     insert_block++;
        // }

        // initial insert
        if (_blocks[insert_block].Count - index > 0) {
            var preserve_slice = new T[_blocks[insert_block].Count - index];
            _blocks[insert_block][insert_index..]
        }

        // uninterrupted insert
        while (items.Length - inserted < _block_size) {
            _blocks.Insert(insert_block, new(_block_size));
            _blocks[insert_block].AddRange(items[inserted..(inserted + _block_size)]);
            inserted += _block_size;
            insert_block++;
        }

        // trailing insert
        var remainder = items.Length - inserted;
        if (_blocks[insert_block].FreeSpace >= remainder) _blocks[insert_block].InsertRange(0, items[^remainder..]);
        else {
            _blocks.Insert(insert_block, new(_block_size));
            inserted += _block_size;
        }
    }

    public void Clear() {
        Count = 0;
        _blocks.Clear();
        _blocks.Add(new(_block_size));
    }

    public bool Contains(T item) =>
        IndexOf(item) != -1;

    public void CopyTo(T[] array, int array_index) {
        foreach (var b in _blocks) {
            b.CopyTo(array, array_index);
            array_index += b.Count;
        }
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

    public ulong GetHash() => 
        KnuthHash.Calculate(_blocks.Select(x => x.GetBlockHash()));

    (int Index, int Block) MapBlockIndex(int index) {
        if (index < 0 || index >= Count) throw new IndexOutOfRangeException();
        int block = 0;
        do {
            if (index < _blocks[block].Count) return (index, block);
            index -= _blocks[block].Count;
            block++;
        } while (index >= 0);
        throw new IndexOutOfRangeException();
    }
}
