#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

internal class TotalCountCompatibilityConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            return JsonSerializer.Deserialize<object?>(ref reader, options);

        var jsonDocument = JsonDocument.ParseValue(ref reader).RootElement;
        return jsonDocument.GetProperty("value").Deserialize<object?>(options);
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}