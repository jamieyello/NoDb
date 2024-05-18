using SlothSerializer;

namespace NoDb.Difference;

public class DifferenceWatcherEventArgs<T> : EventArgs
{
    public required T Value { get; init; }
    public required BitBuilderReader Reader { get; init; }
}
