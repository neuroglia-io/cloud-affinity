namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to describe a correlated cloud event
/// </summary>
[DataContract]
public record CorrelatedCloudEvent
{

    /// <summary>
    /// Initializes a new <see cref="CorrelatedCloudEvent"/>
    /// </summary>
    public CorrelatedCloudEvent() { }

    /// <summary>
    /// Initializes a new <see cref="CorrelatedCloudEvent"/>
    /// </summary>
    /// <param name="criterionName">The name of the correlation criterion the cloud event has fulfilled in the described context</param>
    /// <param name="criterionRuleName">The name of the correlation criterion rule the cloud event has fulfilled in the described context</param>
    /// <param name="e">The correlated cloud event</param>
    public CorrelatedCloudEvent(string criterionName, string criterionRuleName, CloudEvent e)
    {
        if (string.IsNullOrWhiteSpace(criterionName)) throw new ArgumentNullException(nameof(criterionName));
        if (string.IsNullOrWhiteSpace(criterionRuleName)) throw new ArgumentNullException(nameof(criterionRuleName));
        this.CorrelatedAt = DateTimeOffset.Now;
        this.CriterionName = criterionName;
        this.CriterionRuleName = criterionRuleName;
        this.Event = e ?? throw new ArgumentNullException(nameof(e));
    }

    /// <summary>
    /// Gets/sets the date and time the cloud event has been correlated at
    /// </summary>
    [Required]
    [DataMember(Order = 1, Name = "correlatedAt", IsRequired = true), JsonPropertyOrder(1), JsonPropertyName("correlatedAt"), YamlMember(Order = 1, Alias = "correlatedAt")]
    public virtual DateTimeOffset CorrelatedAt { get; set; }

    /// <summary>
    /// Gets/sets the name of the correlation criterion the cloud event has fulfilled in the described context
    /// </summary>
    [Required, MinLength(3)]
    [DataMember(Order = 2, Name = "criterionName", IsRequired = true), JsonPropertyOrder(2), JsonPropertyName("criterionName"), YamlMember(Order = 2, Alias = "criterionName")]
    public virtual string CriterionName { get; set; } = null!;

    /// <summary>
    /// Gets/sets the name of the correlation criterion rule the cloud event has fulfilled in the described context
    /// </summary>
    [Required, MinLength(3)]
    [DataMember(Order = 3, Name = "criterionRuleName", IsRequired = true), JsonPropertyOrder(3), JsonPropertyName("criterionRuleName"), YamlMember(Order = 3, Alias = "criterionRuleName")]
    public virtual string CriterionRuleName { get; set; } = null!;

    /// <summary>
    /// Gets/sets the correlated cloud event
    /// </summary>
    [Required]
    [DataMember(Order = 4, Name = "event", IsRequired = true), JsonPropertyOrder(4), JsonPropertyName("event"), YamlMember(Order = 4, Alias = "event")]
    public virtual CloudEvent Event { get; set; } = null!;

}
