namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to filter a specific cloud event based on a specific context attribute
/// </summary>
[DataContract]
public record CloudEventContextAttributeFilter
{

    /// <summary>
    /// Initializes a new <see cref="CloudEventContextAttributeFilter"/>
    /// </summary>
    public CloudEventContextAttributeFilter() { }

    /// <summary>
    /// Initializes a new <see cref="CloudEventContextAttributeFilter"/>
    /// </summary>
    /// <param name="name">The name of the context attribute filter cloud events must define</param>
    /// <param name="pattern">The Regular Expression pattern used to determine whether or not the value of the specified context attribute matches, if any</param>
    public CloudEventContextAttributeFilter(string name, string? pattern = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Name = name;
        this.Pattern = pattern;
    }

    /// <summary>
    /// Gets/sets the name of the context attribute filter cloud events must define
    /// </summary>
    [Required, MinLength(1)]
    [DataMember(Order = 1, Name = "name", IsRequired = true), JsonPropertyOrder(1), JsonPropertyName("name"), YamlMember(Order = 1, Alias = "name")]
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// Gets/sets the Regular Expression pattern used to determine whether or not the value of the specified context attribute matches, if any.<para></para>
    /// If not set, the context attribute will match if filtered cloud events define it, regardless of the value
    /// </summary>
    [DataMember(Order = 2, Name = "pattern"), JsonPropertyOrder(2), JsonPropertyName("pattern"), YamlMember(Order = 2, Alias = "pattern")]
    public virtual string? Pattern { get; set; }

}