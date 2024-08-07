using System.Collections;

namespace SlothSerializer.Internal;

// This might not be so optimized right now.

// I unfortunately don't think I even need this.

// internal class StorageSector<T> : IList<T> {
//     readonly List<StorageBlock<T>> _storage;
//     readonly int _sector_size = StorageBlock<T>.DEFAULT_BLOCK_SIZE;
//     StorageBlock<T> _start_block;
//     int _start_index;

//     ulong _hash;
//     bool _needs_hash_update = true;
//     readonly bool _cast_to_ulong;

//     public int Count { get; private set; }
//     public bool IsReadOnly => false;

//     public T this[int index] { 
//         get {
//             var block_index = MapBlockIndex(index);
//             return block_index.Block[block_index.Index];
//         }
//         set {
//             var block_index = MapBlockIndex(index);
//             block_index.Block[block_index.Index] = value;
//         }
//     }

//     public StorageSector(
//         List<StorageBlock<T>> storage, 
//         StorageBlock<T> start_block,
//         int start_index) 
//     {
//         _cast_to_ulong = typeof(ulong).IsAssignableFrom(typeof(T));
//         _storage = storage;
//         _start_block = start_block ?? throw new ArgumentNullException(nameof(start_block));
//         _start_index = start_index;
//     }

//     public int IndexOf(T item) {
//         int i = 0;
//         foreach (var existing in this) {
//             if ((object?)existing == (object?)item) return i;
//             i++;
//         }
//         return -1;
//     }

//     public void Insert(int index, T item)
//     {
//         throw new NotImplementedException();
//     }

//     public void RemoveAt(int index)
//     {
//         throw new NotImplementedException();
//     }

//     public void Add(T item)
//     {
//         throw new NotImplementedException();
//     }

//     public void Clear()
//     {
//         throw new NotImplementedException();
//     }

//     public bool Contains(T item)
//     {
//         throw new NotImplementedException();
//     }

//     public void CopyTo(T[] array, int arrayIndex)
//     {
//         throw new NotImplementedException();
//     }

//     public bool Remove(T item)
//     {
//         throw new NotImplementedException();
//     }

//     public IEnumerator<T> GetEnumerator()
//     {
//         var count = 0;
//         var start_index = _start_index;
//         var block = _start_block;
//         while (count < Count) {
//             for (int i = start_index; i < block.Count; i++) {
//                 yield return block[i];
//                 if (++count == Count) yield break;
//             }
//             block = block.NextBlock ?? throw new Exception();
//             start_index = 0;
//         }
//     }

//     IEnumerator IEnumerable.GetEnumerator() =>
//         GetEnumerator();

//     public ulong GetSectorHash() {
//         if (!_needs_hash_update) return _hash;
//         _hash = 
//             _cast_to_ulong ? KnuthHash.Calculate(this.Cast<ulong>()) :
//             KnuthHash.Calculate(this.Select(x => x?.GetHashCode() ?? 0));
//         _needs_hash_update = false;
//         return _hash;
//     }

//     (int Index, StorageBlock<T> Block) MapBlockIndex(int index) {
//         if (index < 0 || index >= Count) throw new IndexOutOfRangeException();
//         var block = _start_block;
//         do {
//             if (index < block.Count) return (index, block);
//             index -= block.Count;
//             block = block.NextBlock ?? throw new Exception("Internal error.");
//         } while (index >= 0);
//         throw new IndexOutOfRangeException();
//     }
// }