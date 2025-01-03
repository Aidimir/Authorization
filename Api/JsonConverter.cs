using System.Globalization;
using Newtonsoft.Json;

namespace Api;

public class DateOnlyConverter : JsonConverter<DateOnly>
{
    private const string DateFormat = "dd-MM-yyyy"; // Формат даты

    public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var dateStr = reader.Value.ToString();
        return DateOnly.ParseExact(dateStr, DateFormat, CultureInfo.InvariantCulture);
    }

    public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
    }
}

