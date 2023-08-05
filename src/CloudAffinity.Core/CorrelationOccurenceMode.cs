namespace CloudAffinity;

/// <summary>
/// Enumerates all supported correlation occurence modes
/// </summary>
public static class CorrelationOccurenceMode
{

    /// <summary>
    /// Specifies that the correlation can only occur once
    /// </summary>
    public const string Single = "single";
    /// <summary>
    /// Specifies that the correlation can occur multiple times
    /// </summary>
    public const string Multiple = "multiple";

}