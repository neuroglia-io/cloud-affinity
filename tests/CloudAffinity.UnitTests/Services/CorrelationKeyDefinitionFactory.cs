using CloudAffinity.Data;

namespace CloudAffinity.UnitTests.Services;

public static class CorrelationKeyDefinitionFactory
{

    public static CorrelationKeyDefinition Create() => new("fake-key", new() { ContextAttribute = "subject" });

}
