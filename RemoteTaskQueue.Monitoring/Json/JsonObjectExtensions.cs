using System.Linq;

using JetBrains.Annotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json
{
    public static class JsonObjectExtensions
    {
        [NotNull]
        public static string ToPrettyJson<T>([CanBeNull] this T o, [NotNull] params JsonConverter[] converters)
        {
            converters = converters.Concat(new JsonConverter[]
                {
                    new StringEnumConverter(),
                    new TimestampJsonConverter(),
                    new TimeGuidJsonConverter()
                }).ToArray();
            return JsonConvert.SerializeObject(o, Formatting.Indented, converters);
        }
    }
}