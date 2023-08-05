namespace CloudAffinity;

/// <summary>
/// Enumerares all supported fulfillment conditions
/// </summary>
public static class FulfillmentCondition
{

    /// <summary>
    /// Indicates that all criteria/rules must be met to fulfill
    /// </summary>
    public const string All = "all";
    /// <summary>
    /// Indicates that any criterion/rule must be met to fulfill
    /// </summary>
    public const string Any = "any";

}