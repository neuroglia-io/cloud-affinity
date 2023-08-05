namespace CloudAffinity.Application;

/// <summary>
/// Defines extensions for <see cref="CorrelationCriterion"/>s
/// </summary>
public static class CorrelationCriterionExtensions
{

    /// <summary>
    /// Determines whether or not the <see cref="CorrelationCriterion"/> matches the specified <see cref="CloudEvent"/>
    /// </summary>
    /// <param name="criterion">The extended <see cref="CorrelationCriterion"/></param>
    /// <param name="e">The <see cref="CloudEvent"/> to check</param>
    /// <param name="expressionEvaluator">The service used to evaluate runtime expressions</param>
    /// <returns>A boolean indicating whether or not the <see cref="CorrelationCriterion"/> matches the specified <see cref="CloudEvent"/></returns>
    public static bool Matches(this CorrelationCriterion criterion, CloudEvent e, IExpressionEvaluator expressionEvaluator)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        return criterion.Rules.Any(r => r.Matches(e, expressionEvaluator));
    }

}