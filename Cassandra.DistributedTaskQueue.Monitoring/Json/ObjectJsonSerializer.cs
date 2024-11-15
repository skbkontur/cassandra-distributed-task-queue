using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

public class TaskDataJsonSerializer : JsonConverter<IRtqTaskData>
{
    public override IRtqTaskData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, IRtqTaskData value, JsonSerializerOptions options)
    {
        JsonSerializerOptions jsonSerializerOptions = new();
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        jsonSerializerOptions.Converters.Add(new TimeGuidJsonConverter());
        jsonSerializerOptions.Converters.Add(new TimestampJsonConverter());

        JsonSerializer.Serialize(writer, value, jsonSerializerOptions);
    }
}