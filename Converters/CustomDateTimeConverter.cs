using System.Text.Json;
using System.Text.Json.Serialization;

namespace CaixaVersoApi.Converters;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "dd/MM/yyyy HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.ParseExact(reader.GetString()!, Format, null);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}

public class CustomNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private const string Format = "dd/MM/yyyy HH:mm:ss";

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        return DateTime.ParseExact(value, Format, null);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(Format));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
