namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to describe the status of a correlation definition
/// </summary>
[DataContract]
public record CorrelationStatus
{

    /// <summary>
    /// Gets/sets the phase the described correlation is in
    /// </summary>
    /// <remarks>See <see cref="CorrelationStatusPhase"/></remarks>
    [DataMember(Order = 1, Name = "phase"), JsonPropertyOrder(1), JsonPropertyName("phase"), YamlMember(Order = 1, Alias = "phase")]
    public virtual string? Phase { get; set; } = null!;

    /// <summary>
    /// Gets/sets a list containing the current correlation occurences, if any
    /// </summary>
    [DataMember(Order = 2, Name = "occurences"), JsonPropertyOrder(2), JsonPropertyName("occurences"), YamlMember(Order = 2, Alias = "occurences")]
    public virtual EquatableList<CorrelationOccurence>? Occurences { get; set; }

}
