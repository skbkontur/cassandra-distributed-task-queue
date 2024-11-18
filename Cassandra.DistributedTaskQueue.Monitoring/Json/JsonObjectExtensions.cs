#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

internal static class JsonObjectExtensions
{
    public static string ToPrettyJson<T>(this T obj)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new TimestampJsonConverter());
        options.Converters.Add(new TimeGuidJsonConverter());

        return JsonSerializer.Serialize(obj, options);
    }
}