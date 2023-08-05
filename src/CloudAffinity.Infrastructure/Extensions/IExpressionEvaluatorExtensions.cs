using CloudAffinity.Infrastructure.Services;

namespace CloudAffinity;

/// <summary>
/// Defines extensions for <see cref="IExpressionEvaluator"/>s
/// </summary>
public static class IExpressionEvaluatorExtensions
{

    /// <summary>
    /// Evaluates the specified condition expression
    /// </summary>
    /// <param name="expressionEvaluator">The service used to evaluate runtime expressions</param>
    /// <param name="expression">The condition expression to evaluate</param>
    /// <param name="data">The data to perform the evaluation against</param>
    /// <param name="args">A key/value mapping of the evaluation arguments, if any</param>
    /// <returns>A boolean indicating whether or not the condition expression matches to the specified data</returns>
    public static bool EvaluateCondition(this IExpressionEvaluator expressionEvaluator, string expression, object data, IDictionary<string, object>? args = null)
    {
        var result = expressionEvaluator.Evaluate(expression, data, args, typeof(bool));
        if (result == null) return false;
        if (result is bool success) return success;
        else return true;
    }

}
