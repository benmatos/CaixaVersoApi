using System.Text.Json;
using System.Text.Json.Serialization;

namespace CaixaVersoApi.Converters;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly string[] Formats =
    [
        "dd/MM/yyyy HH:mm:ss",
        "dd/MM/yyyy",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd"
    ];

    private const string OutputFormat = "dd/MM/yyyy HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()!;
        if (DateTime.TryParseExact(value, Formats, null, System.Globalization.DateTimeStyles.None, out var result))
            return result;
        if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out result))
            return result.ToLocalTime();
        throw new JsonException($"Formato de data inválido: '{value}'. Formatos aceitos: dd/MM/yyyy HH:mm:ss, dd/MM/yyyy, yyyy-MM-dd ou ISO 8601.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(OutputFormat));
    }
}

public class CustomNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private static readonly string[] Formats =
    [
        "dd/MM/yyyy HH:mm:ss",
        "dd/MM/yyyy",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd"
    ];

    private const string OutputFormat = "dd/MM/yyyy HH:mm:ss";

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return null;
        if (DateTime.TryParseExact(value, Formats, null, System.Globalization.DateTimeStyles.None, out var result))
            return result;
        if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out result))
            return result.ToLocalTime();
        throw new JsonException($"Formato de data inválido: '{value}'. Formatos aceitos: dd/MM/yyyy HH:mm:ss, dd/MM/yyyy, yyyy-MM-dd ou ISO 8601.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString(OutputFormat));
        else
            writer.WriteNullValue();
    }
}
