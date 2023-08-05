namespace CloudAffinity.Data;

/// <summary>
/// Represents a resource to configure a service used to correlate cloud events based on managed correlation resources
/// </summary>
[DataContract]
public record Correlator
    : Resource<CorrelatorSpec, CorrelatorStatus>
{

    const string ResourceGroup = "cloud-affinity.io";

    const string ResourceVersion = "v1";

    const string ResourcePlural = "correlators";

    const string ResourceKind = "Correlator";

    /// <summary>
    /// The object used to define the correlator resource type
    /// </summary>
    public static readonly ResourceDefinitionInfo ResourceDefinition = new(ResourceGroup, ResourceVersion, ResourcePlural, ResourceKind);

    /// <inheritdoc/>
    public Correlator() : base(ResourceDefinition) { }

    /// <inheritdoc/>
    public Correlator(ResourceMetadata metadata, CorrelatorSpec spec) : base(ResourceDefinition, metadata, spec) { }

}
