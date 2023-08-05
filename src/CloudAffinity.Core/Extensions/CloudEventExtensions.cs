using CloudAffinity.Data;

namespace CloudAffinity;

/// <summary>
/// Defines extensions for <see cref="CloudEvent"/>s
/// </summary>
public static class CloudEventExtensions
{

    /// <summary>
    /// Attempts to get the value of the specified attribute
    /// </summary>
    /// <param name="e"></param>
    /// <param name="attributeName">The name of the attribute to get</param>
    /// <param name="attributeValue">The value of the attribute to get, in case it exists</param>
    /// <returns>A boolean indicating whether or not the cloud event contains the specified attribute</returns>
    public static bool TryGetAttribute(this CloudEvent e, string attributeName, out object? attributeValue)
    {
        attributeValue = e.GetAttribute(attributeName);
        return attributeValue != null;
    }

}