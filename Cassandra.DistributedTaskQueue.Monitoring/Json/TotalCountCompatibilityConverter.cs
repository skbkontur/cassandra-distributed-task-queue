#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

internal class TotalCountCompatibilityConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            return JsonSerializer.Deserialize<long>(ref reader);

        var jsonDocument = JsonDocument.ParseValue(ref reader).RootElement;
        return jsonDocument.GetProperty("value").Deserialize<long>();
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}