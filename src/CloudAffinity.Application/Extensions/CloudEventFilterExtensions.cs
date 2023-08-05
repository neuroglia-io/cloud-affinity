using System.Text.RegularExpressions;

namespace CloudAffinity.Application;

/// <summary>
/// Defines extensions for <see cref="CloudEventFilter"/>s
/// </summary>
public static class CloudEventFilterExtensions
{

    /// <summary>
    /// Determines whether or not the specified <see cref="CloudEvent"/> is filtered by the <see cref="CloudEventFilter"/> 
    /// </summary>
    /// <param name="filter">The extended <see cref="CloudEventFilter"/></param>
    /// <param name="e">The <see cref="CloudEvent"/> to filter</param>
    /// <param name="expressionEvaluator">The service used to evaluate runtime expressions</param>
    /// <returns>A whether or not the specified <see cref="CloudEvent"/> is filtered by the <see cref="CloudEventFilter"/> </returns>
    public static bool Filters(this CloudEventFilter filter, CloudEvent e, IExpressionEvaluator expressionEvaluator)
    {
        if (filter.Attributes != null)
        {
            foreach (var attribute in filter.Attributes)
            {
                if (!e.TryGetAttribute(attribute.Name, out var value)
                    || string.IsNullOrWhiteSpace(value?.ToString())
                    || (!string.IsNullOrWhiteSpace(attribute.Pattern) && !Regex.IsMatch(value.ToString()!, attribute.Pattern)))
                    return false;
            }
        }
        if (!string.IsNullOrWhiteSpace(filter.Expression) && !expressionEvaluator.EvaluateCondition(filter.Expression, e)) return false;
        return true;
    }

}
