using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

internal class TimestampJsonConverter : JsonConverter<Timestamp>
{
    public override Timestamp Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetDateTime(out var dateTime))
        {
            return new Timestamp(dateTime);
        }
        throw new JsonException($"Unexpected token when parsing timestamp. Expected Date or Integer with value type long, got {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, Timestamp value, JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.ToDateTime());
    }
}