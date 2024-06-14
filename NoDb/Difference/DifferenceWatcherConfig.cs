using SlothSerializer.Internal;

namespace NoDb.Difference;

public class DifferenceWatcherConfig
{
    /// <summary> How often changes should be checked for. Leave null to never check automatically. </summary>
    public TimeSpan? AutoSyncInterval { get; init; }

    /// <summary>
    /// If set to true, an update event will be triggered on the first check.
    /// </summary>
    public bool TriggerInitial { get; init; }

    public BinaryDiff.DiffMethodType DiffMethod { get; init; } = BinaryDiff.DiffMethodType.replace;
}
