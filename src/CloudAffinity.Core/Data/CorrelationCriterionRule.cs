namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to configure a rule of a correlation criterion
/// </summary>
[DataContract]
public record CorrelationCriterionRule
{

    /// <summary>
    /// Initializes a new <see cref="CorrelationCriterionRule"/>
    /// </summary>
    public CorrelationCriterionRule() { }

    /// <summary>
    /// Initializes a new <see cref="CorrelationCriterionRule"/>
    /// </summary>
    /// <param name="name">The name that uniquely identifies the rule with the criterion it belongs to</param>
    /// <param name="correlation">An object used to configure how to correlate filtered cloud events</param>
    /// <param name="filter">An object used to configure the events the rule applies to, if any</param>
    /// <param name="deduplication">An object used to configure the cloud event deduplication strategy to use, if any</param>
    public CorrelationCriterionRule(string name, CloudEventCorrelationRule correlation, CloudEventFilter? filter = null, CloudEventDeduplicationPolicy? deduplication = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Name = name;
        this.Correlation = correlation ?? throw new ArgumentNullException(nameof(filter));
        this.Filter = filter;
        this.Deduplication = deduplication;
    }

    /// <summary>
    /// Gets/sets the name that uniquely identifies the rule with the criterion it belongs to
    /// </summary>
    [Required, MinLength(3)]
    [DataMember(Order = 1, Name = "name", IsRequired = true), JsonPropertyOrder(1), JsonPropertyName("name"), YamlMember(Order = 1, Alias = "name")]
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// Gets/sets an object used to configure how to correlate filtered cloud events
    /// </summary>
    [Required]
    [DataMember(Order = 2, Name = "correlation", IsRequired = true), JsonPropertyOrder(2), JsonPropertyName("correlation"), YamlMember(Order = 2, Alias = "correlation")]
    public virtual CloudEventCorrelationRule Correlation { get; set; } = null!;

    /// <summary>
    /// Gets/sets an object used to configure the events the rule applies to, if any
    /// </summary>
    [DataMember(Order = 3, Name = "filter"), JsonPropertyOrder(3), JsonPropertyName("filter"), YamlMember(Order = 3, Alias = "filter")]
    public virtual CloudEventFilter? Filter { get; set; }

    /// <summary>
    /// Gets/sets an object used to configure the cloud event deduplication strategy to use, if any
    /// </summary>
    [DataMember(Order = 4, Name = "deduplication"), JsonPropertyOrder(4), JsonPropertyName("deduplication"), YamlMember(Order = 4, Alias = "deduplication")]
    public virtual CloudEventDeduplicationPolicy? Deduplication { get; set; }

}
