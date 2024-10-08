﻿using SlothSerializer.DiffTracking;

namespace NoDb.Difference;

public class DifferenceWatcherEventArgs<T> : EventArgs
{
    public required T? Value { get; init; }
    public required BitBuilderDiff Diff { get; init; }
}
