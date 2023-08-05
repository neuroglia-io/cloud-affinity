namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to define how to resolve the value of a defined correlation key
/// </summary>
[DataContract]
public record CorrelationKeyValueResolutionPolicy
{

    /// <summary>
    /// Gets/sets the name of the cloud event context attribute to extract the correlation key value from
    /// </summary>
    [DataMember(Order = 1, Name = "contextAttribute"), JsonPropertyOrder(1), JsonPropertyName("contextAttribute"), YamlMember(Order = 1, Alias = "contextAttribute")]
    public virtual string? ContextAttribute { get; set; }

    /// <summary>
    /// Gets/sets the runtime expression used to resolve the correlation key value of processed cloud events
    /// </summary>
    [DataMember(Order = 2, Name = "expression"), JsonPropertyOrder(2), JsonPropertyName("expression"), YamlMember(Order = 2, Alias = "expression")]
    public virtual string? Expression { get; set; }

}
