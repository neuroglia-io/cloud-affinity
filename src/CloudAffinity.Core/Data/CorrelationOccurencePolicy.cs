namespace CloudAffinity.Data;

/// <summary>
/// Represents an object used to configure the occurences of a correlation
/// </summary>
[DataContract]
public record CorrelationOccurencePolicy
{

    /// <summary>
    /// Initializes a new <see cref="CorrelationOccurencePolicy"/>
    /// </summary>
    public CorrelationOccurencePolicy() { }

    /// <summary>
    /// Initializes a new <see cref="CorrelationOccurencePolicy"/>
    /// </summary>
    /// <param name="mode">A value that configures how the correlation can occur</param>
    public CorrelationOccurencePolicy(string mode)
    {
        if (string.IsNullOrWhiteSpace(mode)) throw new ArgumentNullException(nameof(mode));
        this.Mode = mode;
    }

    /// <summary>
    /// Gets/sets a value that configures how the correlation can occur
    /// </summary>
    /// <remarks>See <see cref="CorrelationOccurenceMode"/></remarks>
    [Required, MinLength(3)]
    [DataMember(Order = 1, Name = "mode", IsRequired = true), JsonPropertyOrder(1), JsonPropertyName("mode"), YamlMember(Order = 1, Alias = "mode")]
    public virtual string Mode { get; set; } = null!;

    /// <summary>
    /// Gets/sets the maximum number of occurences before fulfilling the correlation, if any.<para></para>
    /// Ignored when `occurenceMode` is not set to `multiple`.
    /// </summary>
    [DataMember(Order = 2, Name = "limit"), JsonPropertyOrder(2), JsonPropertyName("limit"), YamlMember(Order = 2, Alias = "limit")]
    public virtual uint? Limit { get; set; }

    /// <summary>
    /// Gets/sets the degree of parallelism of occurences, if any, for the configured correlation<para></para>
    /// If not set, the correlation can have a limitless amount of parallel occurences
    /// </summary>
    [DataMember(Order = 3, Name = "parallelism"), JsonPropertyOrder(3), JsonPropertyName("parallelism"), YamlMember(Order = 3, Alias = "parallelism")]
    public virtual uint? Parallelism { get; set; }

}