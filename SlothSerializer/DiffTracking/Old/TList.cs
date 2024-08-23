// using System.Collections;

// namespace SlothSerializer.DiffTracking;

// /// <summary> A tracked List. </summary>
// public class TList<T> : IList<T> {
//     readonly List<T> _list = new();
//     readonly List<ChangeLogIndex<int, T>> _change_log = new();
//     readonly bool _is_primative;

//     public T this[int index] { 
//         get => _list[index]; 
//         set {
//             if (_is_primative && (object?)value == (object?)_list[index]) return;
//             _change_log.Add(new(index, ChangeActionType.Set, value));
//             _list[index] = value;
//         }
//     }
//     public int Count => _list.Count;
//     public bool IsReadOnly => false;

//     public TList() =>
//         _is_primative = typeof(T).IsPrimitive || (typeof(T) == typeof(string));

//     public TList(IEnumerable<T> add) : this() =>
//         _list.AddRange(add);

//     public void Add(T item) {
//         _change_log.Add(new(Count, ChangeActionType.Add, item));
//         _list.Add(item);
//     }

//     public void Clear() {
//         _change_log.Add(new(Count, ChangeActionType.Clear));
//         _list.Clear();
//     }

//     public bool Contains(T item) =>
//         _list.Contains(item);

//     public void CopyTo(T[] array, int array_index) =>
//         _list.CopyTo(array, array_index);

//     public IEnumerator<T> GetEnumerator() =>
//         _list.GetEnumerator();

//     public int IndexOf(T item) =>
//         _list.IndexOf(item);

//     public void Insert(int index, T item) {
//         _change_log.Add(new(index, ChangeActionType.Insert, item));
//         _list.Insert(index, item);
//     }

//     public bool Remove(T item) {
//         var index = _list.IndexOf(item);
//         if (index == -1) return false;
//         RemoveAt(index);
//         return true;
//     }

//     public void RemoveAt(int index) {
//         _change_log.Add(new(index, ChangeActionType.Remove));
//         throw new NotImplementedException();
//     }

//     IEnumerator IEnumerable.GetEnumerator() =>
//         _list.GetEnumerator();

//     public IEnumerable<ChangeLogIndex<int, T>> ViewChangeList() =>
//         _change_log.AsEnumerable();

//     public void ClearChangeList() =>
//         _change_log.Clear();
// }