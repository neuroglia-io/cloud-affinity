namespace CloudAffinity;

/// <summary>
/// Enumerates the supported status phase of a correlation occurence
/// </summary>
public static class CorrelationOccurenceStatusPhase
{

    /// <summary>
    /// Indicates that the correlation occurence is active and has correlated at least one cloud event
    /// </summary>
    public const string Correlating = "correlating";
    /// <summary>
    /// Indicates that the correlation occurence has fulfilled its criteria
    /// </summary>
    public const string Fulfilled = "fulfilled";
    /// <summary>
    /// Indicates that the correlation occurence has expired
    /// </summary>
    public const string Expired = "expired";

}
