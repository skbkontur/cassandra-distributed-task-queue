using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#nullable enable
namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json
{
    internal class TotalCountCompatibilityConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
                return JObject.Load(reader)["value"]?.ToObject(objectType);
            return reader.Value;
        }

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => true;
    }
}