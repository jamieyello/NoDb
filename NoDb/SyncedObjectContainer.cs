namespace NoDb;

/// <summary> This allows references to be kept. </summary>
internal class SyncedObjectContainer<T> {
    public T? Value { get; set; }

    public SyncedObjectContainer(T? value) => 
        Value = value;
}