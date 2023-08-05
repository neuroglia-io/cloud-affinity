namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to filter cloud events
/// </summary>
[DataContract]
public record CloudEventFilter
{

    /// <summary>
    /// Gets/sets a list containing the context attributes to filter cloud events by
    /// </summary>
    [DataMember(Order = 1, Name = "attributes"), JsonPropertyOrder(1), JsonPropertyName("attributes"), YamlMember(Order = 1, Alias = "attributes")]
    public virtual EquatableList<CloudEventContextAttributeFilter>? Attributes { get; set; }

    /// <summary>
    /// Gets/sets a runtime expression condition used to filter cloud events
    /// </summary>
    [DataMember(Order = 2, Name = "expression"), JsonPropertyOrder(2), JsonPropertyName("expression"), YamlMember(Order = 2, Alias = "expression")]
    public virtual string? Expression { get; set; }

}
