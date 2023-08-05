namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to configure a cloud event deduplication policy
/// </summary>
[DataContract]
public record CloudEventDeduplicationPolicy
{

    /// <summary>
    /// Gets/sets the deduplication strategy to use
    /// </summary>
    /// <remarks>See <see cref="CloudEventDeduplicationPolicyStrategy"/></remarks>
    [Required, MinLength(3)]
    [DataMember(Order = 1, Name = "strategy", IsRequired = true), JsonPropertyOrder(1), JsonPropertyName("strategy"), YamlMember(Order = 1, Alias = "strategy")]
    public virtual string Strategy { get; set; } = null!;

    /// <summary>
    /// Gets/sets the rolling duration to wait for accepting new duplicate cloud events.<para></para>
    /// Required if `strategy` has been set to `throttle`, otherwise ignored.
    /// </summary>
    [DataMember(Order = 2, Name = "rollingDuration"), JsonPropertyOrder(2), JsonPropertyName("rollingDuration"), YamlMember(Order = 2, Alias = "rollingDuration")]
    public virtual TimeSpan? RollingDuration { get; set; }

}
