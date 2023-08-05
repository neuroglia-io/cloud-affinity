namespace CloudAffinity;

/// <summary>
/// Enumerates the supported status phases of a correlation
/// </summary>
public static class CorrelationStatusPhase
{

    /// <summary>
    /// Indicates that the correlation has been defined and initiated, but the correlation process has not started yet or is in a waiting state.
    /// </summary>
    public const string Pending = "pending";
    /// <summary>
    /// Indicates that the correlation process is actively correlating events or data based on the defined correlation criteria and rules.
    /// </summary>
    public const string Correlating = "correlating";
    /// <summary>
    /// Indicates that a user has manually stopped the correlation process, and will no longer occur.
    /// </summary>
    public const string Stopped = "stopped";
    /// <summary>
    /// Indicates that a correlation has expired, and will no longer occur.
    /// </summary>
    public const string Expired = "expired";
    /// <summary>
    /// Indicates that a correlation has successfully identified the expected relationships or patterns between events and has achieved its purpose
    /// </summary>
    public const string Fulfilled = "fulfilled";

}
