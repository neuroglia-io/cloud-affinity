namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to configure a cloud event correlation rule
/// </summary>
[DataContract]
public record CloudEventCorrelationRule
{

    /// <summary>
    /// Initializes a new <see cref="CloudEventCorrelationRule"/>
    /// </summary>
    public CloudEventCorrelationRule() { }

    /// <summary>
    /// Initializes a new <see cref="CloudEventCorrelationRule"/>
    /// </summary>
    /// <param name="keys">A list containing the definition of the correlation keys to use to correlate filtered cloud events</param>
    public CloudEventCorrelationRule(params CorrelationKeyDefinition[] keys)
    {
        if (keys == null || !keys.Any()) throw new ArgumentNullException(nameof(keys));
        this.Keys = new(keys);
    }

    /// <summary>
    /// Gets/sets a list containing the definition of the correlation keys to use to correlate filtered cloud events
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Order = 1, Name = "keys", IsRequired = true), JsonPropertyOrder(1), JsonPropertyName("keys"), YamlMember(Order = 1, Alias = "keys")]
    public virtual EquatableList<CorrelationKeyDefinition> Keys { get; set; } = null!;

    /// <summary>
    /// Gets/sets a runtime expression, if any, used to determine whether or not to correlate a cloud event against an occurence<para></para>
    /// The runtime expression receives the cloud event as input and the occurence as '$OCCURENCE' argument
    /// </summary>
    [DataMember(Order = 2, Name = "condition"), JsonPropertyOrder(2), JsonPropertyName("condition"), YamlMember(Order = 2, Alias = "condition")]
    public virtual string? Condition { get; set; }

}
