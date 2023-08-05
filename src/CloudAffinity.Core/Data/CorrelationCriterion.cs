namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to configure a criterion that must be fulfilled by cloud events to correlate
/// </summary>
[DataContract]
public record CorrelationCriterion
{

    /// <summary>
    /// Initializes a new <see cref="CorrelationCriterion"/>
    /// </summary>
    public CorrelationCriterion() { }

    /// <summary>
    /// Initializes a new <see cref="CorrelationCriterion"/>
    /// </summary>
    /// <param name="name">The name that uniquely identifies the criterion with the correlation it belongs to</param>
    /// <param name="fulfillmentCondition">A value that specifies the condition based on which to determine if the criterion has been fulfilled</param>
    /// <param name="rules">A list containing the rules the criterion is made out of and which needs to be met for the criterion to be fulfilled</param>
    public CorrelationCriterion(string name, string fulfillmentCondition, IEnumerable<CorrelationCriterionRule> rules)
    {
        if(string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrWhiteSpace(fulfillmentCondition)) throw new ArgumentNullException(nameof(fulfillmentCondition));
        if (rules == null || !rules.Any()) throw new ArgumentNullException(nameof(rules));
        this.Name = name;
        this.FulfillmentCondition = fulfillmentCondition;
        this.Rules = new(rules);
    }

    /// <summary>
    /// Gets/sets the name that uniquely identifies the criterion with the correlation it belongs to
    /// </summary>
    [Required, MinLength(3)]
    [DataMember(Order = 1, Name = "name", IsRequired = true), JsonPropertyOrder(1), JsonPropertyName("name"), YamlMember(Order = 1, Alias = "name")]
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// Gets/sets a value that specifies the condition based on which to determine if the criterion has been fulfilled
    /// </summary>
    /// <remarks>See <see cref="CloudAffinity.FulfillmentCondition"/></remarks>
    [Required, MinLength(3)]
    [DataMember(Order = 2, Name = "fulfillmentCondition", IsRequired = true), JsonPropertyOrder(2), JsonPropertyName("fulfillmentCondition"), YamlMember(Order = 2, Alias = "fulfillmentCondition")]
    public virtual string FulfillmentCondition { get; set; } = null!;

    /// <summary>
    /// Gets/sets a list containing the rules the criterion is made out of and which needs to be met for the criterion to be fulfilled
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Order = 3, Name = "rules", IsRequired = true), JsonPropertyOrder(3), JsonPropertyName("rules"), YamlMember(Order = 3, Alias = "rules")]
    public virtual EquatableList<CorrelationCriterionRule> Rules { get; set; } = null!;

}