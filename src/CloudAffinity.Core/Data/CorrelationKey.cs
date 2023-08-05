namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to describe a correlation key
/// </summary>
[DataContract]
public record CorrelationKey
{

    /// <summary>
    /// Initializes a new <see cref="CorrelationKey"/>
    /// </summary>
    public CorrelationKey() { }

    /// <summary>
    /// Initializes a new <see cref="CorrelationKey"/>
    /// </summary>
    /// <param name="name">The name of the described correlation key</param>
    /// <param name="value">The value of the described correlation key</param>
    public CorrelationKey(string name, string value)
    {
        if(string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        if(string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
        this.Name = name;
        this.Value = value;
    }

    /// <summary>
    /// Gets/sets the name of the described correlation key
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// Gets/sets the value of the described correlation key
    /// </summary>
    public virtual string Value { get; set; } = null!;

}