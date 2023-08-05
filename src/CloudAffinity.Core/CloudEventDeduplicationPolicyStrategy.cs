namespace CloudAffinity;

/// <summary>
/// Enumerates all supported cloud event deduplication policy strategies
/// </summary>
public static class CloudEventDeduplicationPolicyStrategy
{

    /// <summary>
    /// Indicates that the duplicate cloud event should overwrite the original one
    /// </summary>
    public const string Overwrite = "overwrite";
    /// <summary>
    /// Indicates that duplicate cloud events should be throttled
    /// </summary>
    public const string Throttle = "throttle";

}