namespace CloudAffinity.Application;

/// <summary>
/// Defines extensions for <see cref="CorrelationCriterionRule"/>s
/// </summary>
public static class CorrelationCriterionRuleExtensions
{

    /// <summary>
    /// Determines whether or not the specified <see cref="CloudEvent"/> matches the criterion rule
    /// </summary>
    /// <param name="rule">The extended <see cref="CorrelationCriterionRule"/></param>
    /// <param name="e">The <see cref="CloudEvent"/> to check</param>
    /// <param name="expressionEvaluator">The service used to evaluate runtime expressions</param>
    /// <returns>A boolean indicating whether or not the specified <see cref="CloudEvent"/> matches the criterion rule</returns>
    public static bool Matches(this CorrelationCriterionRule rule, CloudEvent e, IExpressionEvaluator expressionEvaluator)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        if (rule.Filter != null && !rule.Filter.Filters(e, expressionEvaluator)) return false;
        foreach (var correlationKey in rule.Correlation.Keys)
        {
            if (!string.IsNullOrWhiteSpace(correlationKey.ValueFrom.ContextAttribute) && !e.TryGetAttribute(correlationKey.ValueFrom.ContextAttribute, out _)) return false;
        }
        return true;
    }

}
