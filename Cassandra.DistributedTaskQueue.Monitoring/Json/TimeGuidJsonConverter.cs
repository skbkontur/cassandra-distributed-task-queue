using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

internal class TimeGuidJsonConverter : JsonConverter<TimeGuid>
{
    public override TimeGuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var readAsString = reader.GetString();
            return TimeGuid.Parse(readAsString);
        }
        return null;
    }

    public override void Write(Utf8JsonWriter writer, TimeGuid value, JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.ToGuid());
    }
}