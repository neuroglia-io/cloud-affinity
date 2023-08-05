namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to define a correlation key
/// </summary>
[DataContract]
public record CorrelationKeyDefinition
{

    /// <summary>
    /// Initializes a new <see cref="CorrelationKeyDefinition"/>
    /// </summary>
    public CorrelationKeyDefinition() { }

    /// <summary>
    /// Initializes a new <see cref="CorrelationKeyDefinition"/>
    /// </summary>
    /// <param name="name">The name of the defined correlation key</param>
    /// <param name="valueFrom">An object used to configure how to resolve the value of the defined correlation key when correlating cloud events</param>
    public CorrelationKeyDefinition(string name, CorrelationKeyValueResolutionPolicy valueFrom)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        Name = name;
        ValueFrom = valueFrom ?? throw new ArgumentNullException(nameof(valueFrom));
    }

    /// <summary>
    /// Gets/sets the name of the defined correlation key
    /// </summary>
    [Required, MinLength(3)]
    [DataMember(Order = 1, Name = "name", IsRequired = true), JsonPropertyOrder(1), JsonPropertyName("name"), YamlMember(Order = 1, Alias = "name")]
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// Gets/sets an object used to configure how to resolve the value of the defined correlation key when correlating cloud events
    /// </summary>
    [Required]
    [DataMember(Order = 2, Name = "valueFrom", IsRequired = true), JsonPropertyOrder(2), JsonPropertyName("valueFrom"), YamlMember(Order = 2, Alias = "valueFrom")]
    public virtual CorrelationKeyValueResolutionPolicy ValueFrom { get; set; } = null!;

}
