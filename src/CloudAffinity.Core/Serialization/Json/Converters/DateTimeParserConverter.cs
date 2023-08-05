using System.Text.Json;

namespace CloudAffinity.Serialization.Json.Converters;

/// <summary>
/// Represents a <see cref="JsonConverter"/> used to read and write <see cref="DateTime"/>s
/// </summary>
public class DateTimeParserConverter
    : JsonConverter<DateTime>
{

    /// <inheritdoc/>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => DateTime.Parse(reader.GetString() ?? string.Empty);

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());

}