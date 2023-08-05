namespace CloudAffinity.Application.Configuration;

/// <summary>
/// Represents the options used to configure a Cloud Affinity cloud event gateway
/// </summary>
public class CorrelatorOptions
{

    /// <summary>
    /// Gets the prefix for all Cloud Affinity correlator related environment variables
    /// </summary>
    public const string EnvironmentVariablePrefix = "CLOUDAFFINITY_CORRELATOR_";

    /// <summary>
    /// Gets/sets the correlator's name
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// Gets/sets the correlator's namespace
    /// </summary>
    public virtual string? Namespace { get; set; } = null!;

}
