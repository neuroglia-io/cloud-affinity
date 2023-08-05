namespace CloudAffinity.Data;

/// <summary>
/// Represents a resource used to configure the definition of a cloud event correlation
/// </summary>
[DataContract]
public record Correlation
    : Resource<CorrelationSpec, CorrelationStatus>
{

    const string ResourceGroup = "cloud-affinity.io";

    const string ResourceVersion = "v1";

    const string ResourcePlural = "correlations";

    const string ResourceKind = "Correlation";

    /// <summary>
    /// The object used to define the correlation resource type
    /// </summary>
    public static readonly ResourceDefinitionInfo ResourceDefinition = new(ResourceGroup, ResourceVersion, ResourcePlural, ResourceKind);

    /// <inheritdoc/>
    public Correlation() : base(ResourceDefinition) { }

    /// <inheritdoc/>
    public Correlation(ResourceMetadata metadata, CorrelationSpec spec) : base(ResourceDefinition, metadata, spec) { }

}
