using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoomBooking.Api.Infrastructure;

/// <summary>
/// Reads times as either "09:00" or "09:00:00"; always writes "09:00:00".
/// </summary>

public sealed class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private static readonly string[] AcceptedFormats = ["HH:mm", "HH:mm:ss"];

    private const string WriteFormat = "HH:mm:ss";

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.GetString();

        if (TimeOnly.TryParseExact(text, AcceptedFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var time))
        {
            return time;
        }

        throw new JsonException($"'{text}' is not a time. Use HH:mm or HH:mm:ss.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(WriteFormat, CultureInfo.InvariantCulture));
}
