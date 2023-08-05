namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to describe the occurence of a cloud event correlation
/// </summary>
[DataContract]
public record CorrelationOccurence
{

    /// <summary>
    /// Gets/sets the id of the described correlation occurence
    /// </summary>
    [Required, MinLength(3)]
    [DataMember(Order = 1, Name = "id"), JsonPropertyOrder(1), JsonPropertyName("id"), YamlMember(Order = 1, Alias = "id")]
    public virtual string Id { get; set; } = Guid.NewGuid().ToShortString();

    /// <summary>
    /// Gets/sets the current phase of the correlation occurence
    /// </summary>
    /// <remarks>See <see cref="CorrelationOccurenceStatusPhase"/></remarks>
    [Required, MinLength(3)]
    [DataMember(Order = 2, Name = "phase"), JsonPropertyOrder(2), JsonPropertyName("phase"), YamlMember(Order = 2, Alias = "phase")]
    public virtual string Phase { get; set; } = CorrelationOccurenceStatusPhase.Correlating!;

    /// <summary>
    /// Gets/sets a list containing the occurence's correlation keys
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Order = 3, Name = "keys"), JsonPropertyOrder(3), JsonPropertyName("keys"), YamlMember(Order = 3, Alias = "keys")]
    public virtual EquatableList<CorrelationKey> Keys { get; set; } = new();

    /// <summary>
    /// Gets/sets a list containing the events that have been correlated in the occurence
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Order = 4, Name = "events"), JsonPropertyOrder(4), JsonPropertyName("events"), YamlMember(Order = 4, Alias = "events")]
    public virtual EquatableList<CorrelatedCloudEvent> Events { get; set; } = new();

}
