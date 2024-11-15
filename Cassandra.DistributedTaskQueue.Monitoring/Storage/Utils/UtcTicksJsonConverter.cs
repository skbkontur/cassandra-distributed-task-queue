#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Utils;

public class UtcTicksJsonConverter : JsonConverter<object?>
{
    public override bool CanConvert(Type objectType)
    {
        if (objectType == typeof(DateTime) || objectType == typeof(DateTime?))
        {
            return true;
        }
#if HAVE_DATE_TIME_OFFSET
        if (objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?))
        {
            return true;
        }
#endif

        return objectType == typeof(long) || objectType == typeof(long?);
    }

    public override bool HandleNull => true;

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var underlyingType = Nullable.GetUnderlyingType(typeToConvert);
        var type = underlyingType ?? typeToConvert;

        if (type == typeof(long))
        {
            var dateTime = JsonSerializer.Deserialize<DateTime?>(ref reader);
            if (dateTime == null)
            {
                if (underlyingType != null)
                    return null;
                return 0L;
            }
            return dateTime.Value.Ticks;
        }

        return JsonSerializer.Deserialize<DateTime?>(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value is long ticks)
            JsonSerializer.Serialize(writer, CreateDateTime(ticks), options);
        else
            JsonSerializer.Serialize(writer, value, options);
    }

    private static DateTime CreateDateTime(long value)
    {
        if (value < minTicks)
            value = minTicks;
        if (value > maxTicks)
            value = maxTicks;
        return new DateTime(value, DateTimeKind.Utc);
    }

    private static readonly long minTicks = DateTime.MinValue.Ticks;
    private static readonly long maxTicks = DateTime.MaxValue.Ticks;
}