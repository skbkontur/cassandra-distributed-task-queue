#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

internal static class JsonObjectExtensions
{
    public static string ToPrettyJson<T>(this T obj)
    {
        return JsonSerializer.Serialize(obj, jsonSerializerSettings);
    }

    private static readonly JsonSerializerOptions jsonSerializerSettings = new()
        {
            Converters =
                {
                    new JsonStringEnumConverter(),
                    new TimestampJsonConverter(),
                    new TimeGuidJsonConverter()
                }
        };
}