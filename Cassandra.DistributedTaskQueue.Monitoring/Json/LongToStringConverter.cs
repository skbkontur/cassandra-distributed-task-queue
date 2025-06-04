#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

/// <summary>
///     Конвертер, которые сериализует long-и в строки вместо чисел,
///     ибо JSON.parse() на длинных long-ах теряет последние разряды
/// </summary>
public class LongToStringConverter : JsonConverter<long?>
{
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = JsonSerializer.Deserialize<string?>(ref reader);
        if (value == null)
        {
            return null;
        }
        return long.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}