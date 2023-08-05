using System.Text.Json;

namespace CloudAffinity.Serialization.Json.Converters;

/// <summary>
/// Represents a <see cref="JsonConverter"/> used to read and write <see cref="TimeSpan"/>s from and to the ISO 8601 format
/// </summary>
public class Iso8601TimeSpanConverter
    : JsonConverter<TimeSpan>
{

    /// <inheritdoc/>
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var timespanStr = reader.GetString();
        if (string.IsNullOrEmpty(timespanStr)) return default;
        return Iso8601TimeSpan.Parse(timespanStr);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Iso8601TimeSpan.Format(value));
    }

}
