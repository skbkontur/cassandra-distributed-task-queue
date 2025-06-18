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
        JsonSerializer.Serialize(writer, value, staticSerializerOptions);
    }

    private static readonly JsonSerializerOptions staticSerializerOptions = new()
        {
            Converters =
                {
                    new JsonStringEnumConverter(),
                    new TimeGuidJsonConverter(),
                    new TimestampJsonConverter()
                }
        };
}