using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json
{
    public class TaskDataJsonSerializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            staticSerializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IRtqTaskData).IsAssignableFrom(objectType);
        }

        public override bool CanRead => false;

        private static readonly JsonSerializer staticSerializer = new JsonSerializer {Converters = {new StringEnumConverter()}};
    }
}