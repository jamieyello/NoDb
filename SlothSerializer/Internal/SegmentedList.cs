﻿using System.Collections;

namespace SlothSerializer.Internal;

/// <summary>
/// This class brings down the memory usage of List<>, especially with valuetypes/structs,
/// by stringing small arrays together rather than doubling one big one when the capacity is reached.
/// Also optimized for hashing, caching a hash for each segment.
/// </summary>
public class SegmentedList<T> : IList<T> {
    // Base parameters
    /// <summary> The combined arrays. </summary>
    readonly List<SegmentedListBlock<T>> _blocks;

    public int Count { get; private set; }
    public bool IsReadOnly => false;
    readonly int _block_size = SegmentedListBlock<T>.DEFAULT_BLOCK_SIZE;

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
    }

    // Very slow, needs speedup, testing
    public void RemoveAt(int index) {
        throw new NotImplementedException();
    }

    public int IndexOf(T item) {
        int i_c = 0;
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
            for (int i = 0; i < _blocks[i].Count; i++) {
                yield return _blocks[i][b];
                if (c++ < Count) yield break;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public ulong GetHash() => 
        KnuthHash.Calculate(_blocks.Select(x => x.GetBlockHash()));
}
