namespace NoDb.Difference;

public class DifferenceWatcherOptions
{
    /// <summary> How often changes should be checked for. Set to 1 second by default. Set to null to never check automatically. </summary>
    public TimeSpan? SyncInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// If set to true, an update event will be triggered on the first check.
    /// </summary>
    public bool TriggerInitial { get; set; }
}
