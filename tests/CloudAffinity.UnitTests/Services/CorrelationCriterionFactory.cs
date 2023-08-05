using CloudAffinity.Data;
using Hylo;

namespace CloudAffinity.UnitTests.Services;

public static class CorrelationCriterionFactory
{

    public static CorrelationCriterion Create(string fulfillmentCondition, params CloudEvent[] cloudEvents) => new($"criterion-{Guid.NewGuid().ToShortString()}", fulfillmentCondition, cloudEvents.Select(CorrelationCriterionRuleFactory.Create));

    public static CorrelationCriterion Create() => Create(FulfillmentCondition.All, CloudEventFactory.Create());

}
