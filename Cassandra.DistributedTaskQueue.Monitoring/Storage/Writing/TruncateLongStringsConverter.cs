using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing;

public class TruncateLongStringsConverter : JsonConverter<string>
{
    public TruncateLongStringsConverter(int maxStringLength)
    {
        this.maxStringLength = maxStringLength;
    }

    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        var str = value;
        if (str == null || str.Length <= maxStringLength)
            writer.WriteStringValue(str);
        else
            writer.WriteStringValue(str.Substring(0, maxStringLength));
    }

    private readonly int maxStringLength;
}