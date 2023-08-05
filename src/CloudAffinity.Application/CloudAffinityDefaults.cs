using Hylo;

namespace CloudAffinity.Application;

/// <summary>
/// Exposes defaults about Cloud Affinity
/// </summary>
public static class CloudAffinityDefaults
{

    /// <summary>
    /// Gets the default group for Cloud Affinity resources
    /// </summary>
    public const string ResourceGroup = "cloud-streams.io";

    /// <summary>
    /// Exposes Cloud Affinity default resources
    /// </summary>
    public static class Resources
    {

        /// <summary>
        /// Exposes Cloud Affinity resource definitions
        /// </summary>
        public static class Definitions
        {

            /// <summary>
            /// Gets the definition of Correlation resources
            /// </summary>
            public static ResourceDefinition Correlation { get; } = LoadResourceDefinition(nameof(Correlation));

            /// <summary>
            /// Gets the definition of Correlator resources
            /// </summary>
            public static ResourceDefinition Correlator { get; } = LoadResourceDefinition(nameof(Correlator));

            /// <summary>
            /// Gets a new <see cref="IEnumerable{T}"/> containing Cloud Affinity default resource definitions
            /// </summary>
            /// <returns></returns>
            public static IEnumerable<ResourceDefinition> AsEnumerable()
            {
                yield return Correlation;
                yield return Correlator;
            }

            static ResourceDefinition LoadResourceDefinition(string name)
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Definitions", $"{name.ToHyphenCase()}.yaml");
                var yaml = File.ReadAllText(filePath);
                var resourceDefinition = Serializer.Json.Deserialize<ResourceDefinition>(Serializer.Json.Serialize(Serializer.Yaml.Deserialize<IDictionary<string, object>>(yaml)!))!;
                return resourceDefinition;
            }

        }

    }

}
