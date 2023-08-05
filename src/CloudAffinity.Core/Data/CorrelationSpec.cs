namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to configure the desired state of a correlation
/// </summary>
[DataContract]
public record CorrelationSpec
{

    /// <summary>
    /// Initializes a new <see cref="CorrelationSpec"/>
    /// </summary>
    public CorrelationSpec() { }

    /// <summary>
    /// Initializes a new <see cref="CorrelationSpec"/>
    /// </summary>
    /// <param name="occurence">The correlation's occurence mode</param>
    /// <param name="fulfillmentCondition">A value that specifies the condition based on which to determine the phase of defined correlations</param>
    /// <param name="criteria">A list containing the correlation's criteria</param>
    public CorrelationSpec(CorrelationOccurencePolicy occurence, string fulfillmentCondition, IEnumerable<CorrelationCriterion> criteria)
    {
        if (string.IsNullOrWhiteSpace(fulfillmentCondition)) throw new ArgumentNullException(nameof(fulfillmentCondition));
        if (criteria == null || !criteria.Any()) throw new ArgumentNullException(nameof(criteria));
        this.Occurence = occurence ?? throw new ArgumentNullException(nameof(occurence));
        this.FulfillmentCondition = fulfillmentCondition;
        this.Criteria = new(criteria);
    }

    /// <summary>
    /// Gets/sets an object used to configure the correlation's occurence
    /// </summary>
    [Required]
    [DataMember(Order = 1, Name = "occurence", IsRequired = true), JsonPropertyOrder(1), JsonPropertyName("occurence"), YamlMember(Order = 1, Alias = "occurence")]
    public virtual CorrelationOccurencePolicy Occurence { get; set; } = null!;

    /// <summary>
    /// Gets/sets a value that specifies the condition based on which to determine the phase of defined correlations
    /// </summary>
    /// <remarks>See <see cref="CloudAffinity.FulfillmentCondition"/></remarks>
    [Required, MinLength(3)]
    [DataMember(Order = 2, Name = "fulfillmentCondition", IsRequired = true), JsonPropertyOrder(2), JsonPropertyName("fulfillmentCondition"), YamlMember(Order = 2, Alias = "fulfillmentCondition")]
    public virtual string FulfillmentCondition { get; set; } = null!;

    /// <summary>
    /// Gets/sets a list containing the correlation's criteria
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Order = 3, Name = "criteria", IsRequired = true), JsonPropertyOrder(3), JsonPropertyName("criteria"), YamlMember(Order = 3, Alias = "criteria")]
    public virtual EquatableList<CorrelationCriterion> Criteria { get; set; } = null!;

}
