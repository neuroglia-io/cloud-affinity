using CloudAffinity.Data;
using Hylo;

namespace CloudAffinity.UnitTests.Services;

public static class CorrelationCriterionRuleFactory
{

    public static CorrelationCriterionRule Create(CloudEvent cloudEvent)
    {
        var name = $"rule-{Guid.NewGuid().ToShortString()}";
        var correlationRule = new CloudEventCorrelationRule(CorrelationKeyDefinitionFactory.Create());
        var filter = new CloudEventFilter()
        {
            Attributes = new()
            {
                new("source", cloudEvent.Source.OriginalString),
                new("type", cloudEvent.Type)
            }
        };
        return new CorrelationCriterionRule(name, correlationRule, filter);
    }

    public static CorrelationCriterionRule Create() => Create(CloudEventFactory.Create());

}