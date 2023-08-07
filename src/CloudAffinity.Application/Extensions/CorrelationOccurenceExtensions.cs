namespace CloudAffinity.Application;

/// <summary>
/// Defines extensions for <see cref="CorrelationOccurence"/>s
/// </summary>
public static class CorrelationContextExtensions
{

    /// <summary>
    /// Determines whether or not the occurence fulfills the specified criterion 
    /// </summary>
    /// <param name="occurence">The <see cref="CorrelationOccurence"/> to check</param>
    /// <param name="criterion">The <see cref="CorrelationCriterion"/> to check</param>
    /// <returns>A boolean indicating whether or not the occurence fulfills the specified criterion</returns>
    public static bool Fulfills(this CorrelationOccurence occurence, CorrelationCriterion criterion)
    {
        return criterion.FulfillmentCondition switch
        {
            FulfillmentCondition.Any or FulfillmentCondition.Single => occurence.Events.Any(e => e.CriterionName == criterion.Name),
            FulfillmentCondition.All => criterion.Rules.All(r => occurence.Events.Any(e => e.CriterionName == criterion.Name && e.CriterionRuleName == r.Name)),
            _ => false
        };
    }

    /// <summary>
    /// Determines whether or not the occurence fulfills the specified correlation 
    /// </summary>
    /// <param name="occurence">The <see cref="CorrelationOccurence"/> to check</param>
    /// <param name="correlation">The <see cref="Correlation"/> to check</param>
    /// <returns>A boolean indicating whether or not the occurence fulfills the specified correlation</returns>
    public static bool TryFulfill(this CorrelationOccurence occurence, Correlation correlation)
    {
        var fulfills = correlation.Spec.FulfillmentCondition switch
        {
            FulfillmentCondition.Any or FulfillmentCondition.Single  => correlation.Spec.Criteria.Any(occurence.Fulfills),
            FulfillmentCondition.All => correlation.Spec.Criteria.All(occurence.Fulfills),
            _ => false
        };
        if (fulfills) occurence.Phase = CorrelationOccurenceStatusPhase.Fulfilled;
        return fulfills;
    }

    /// <summary>
    /// Attempts to correlate the specified cloud event, given the specified criterion rule
    /// </summary>
    /// <param name="occurence">The current correlation occurence</param>
    /// <param name="cloudEvent">The cloud event to correlate</param>
    /// <param name="criterion">The correlation criterion to correlate the event for</param>
    /// <param name="criterionRule">The correlation rule to use to perform the correlation of the specified cloud event</param>
    /// <param name="expressionEvaluator">The service used to evaluate runtime expressions</param>
    /// <returns>A new <see cref="CorrelationResult"/>, that describes the result of the correlation attempt</returns>
    public static CorrelationResult TryCorrelate(this CorrelationOccurence occurence, CloudEvent cloudEvent, CorrelationCriterion criterion, CorrelationCriterionRule criterionRule, IExpressionEvaluator expressionEvaluator)
    {
        if (cloudEvent == null) throw new ArgumentNullException(nameof(cloudEvent));
        if (criterion == null) throw new ArgumentNullException(nameof(criterion));
        if (criterionRule == null) throw new ArgumentNullException(nameof(criterionRule));

        var duplicateCloudEvent = occurence.Events.FirstOrDefault(e => e.CriterionName == criterion.Name && e.CriterionRuleName == criterionRule.Name);
        if (duplicateCloudEvent != null)
        {
            switch (criterionRule?.Deduplication?.Strategy)
            {
                case CloudEventDeduplicationPolicyStrategy.Overwrite:
                    var index = occurence.Events.IndexOf(duplicateCloudEvent);
                    occurence.Events.Remove(duplicateCloudEvent);
                    var correlatedCloudEvent = new CorrelatedCloudEvent(criterion.Name, criterionRule.Name, cloudEvent);
                    if (index >= occurence.Events.Count) occurence.Events.Add(correlatedCloudEvent);
                    else occurence.Events.Insert(index, correlatedCloudEvent);
                    return CorrelationResult.Deduplicated;
                case CloudEventDeduplicationPolicyStrategy.Throttle:
                    var waitUntil = occurence.Events.Select(e => e.Event.Time).Last() + (criterionRule.Deduplication.RollingDuration ?? TimeSpan.FromSeconds(5));
                    if (!waitUntil.HasValue || waitUntil.Value < DateTimeOffset.Now) return CorrelationResult.Unrelated;
                    else return CorrelationResult.Deduplicated;
                default: return CorrelationResult.Unrelated;
            }
        }

        if (occurence.Fulfills(criterion) && (criterion.FulfillmentCondition != FulfillmentCondition.Any || (criterion.FulfillmentCondition == FulfillmentCondition.Any && duplicateCloudEvent != null))) return CorrelationResult.Unrelated;

        foreach (var keyDefinition in criterionRule.Correlation.Keys)
        {
            if (!string.IsNullOrWhiteSpace(criterionRule.Correlation.Condition))
            {
                var evaluationArguments = new Dictionary<string, object>()
                {
                    { "OCCURENCE", occurence }
                };
                try
                {
                    if (!expressionEvaluator.EvaluateCondition(criterionRule.Correlation.Condition, cloudEvent, evaluationArguments)) return CorrelationResult.Unrelated;
                }
                catch { }
            }

            var key = occurence.Keys.FirstOrDefault(k => k.Name == keyDefinition.Name);
            object? value = null;

            if (!string.IsNullOrWhiteSpace(keyDefinition.ValueFrom.ContextAttribute) && (!cloudEvent.TryGetAttribute(keyDefinition.ValueFrom.ContextAttribute, out value) || value == null)) return CorrelationResult.Unrelated;
            else if (!string.IsNullOrWhiteSpace(keyDefinition.ValueFrom.Expression)) value = expressionEvaluator.Evaluate(keyDefinition.ValueFrom.Expression, cloudEvent, expectedType: typeof(string));

            var valueStr = value?.ToString();
            if (string.IsNullOrWhiteSpace(valueStr)) return CorrelationResult.Unrelated;

            if (key == null) occurence.Keys.Add(new(keyDefinition.Name, valueStr));
            else if (valueStr != key.Value) return CorrelationResult.Unrelated;
        }

        occurence.Events.Add(new(criterion.Name, criterionRule.Name, cloudEvent));

        return CorrelationResult.Correlated;
    }

}
