using Hylo.Serialization;
using Hylo.Serialization.Json;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CloudAffinity.Application;

/// <summary>
/// Enumerates all supported correlation results
/// </summary>
[TypeConverter(typeof(StringEnumTypeConverter))]
[JsonConverter(typeof(JsonStringEnumConverterFactory))]
public enum CorrelationResult
{
    /// <summary>
    /// Indicates that the cloud event could not be correlated
    /// </summary>
    [EnumMember(Value = "unrelated")]
    Unrelated = 1,
    /// <summary>
    /// Indicates that the cloud event has been correlated
    /// </summary>
    [EnumMember(Value = "correlated")]
    Correlated = 2,
    /// <summary>
    /// Indicates that the cloud event has been deduplicated
    /// </summary>
    [EnumMember(Value = "deduplicated")]
    Deduplicated = 4
}
