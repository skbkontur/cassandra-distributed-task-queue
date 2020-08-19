using System;

using Newtonsoft.Json;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json
{
    public class TimeGuidJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
                writer.WriteValue(((TimeGuid)value).ToGuid().ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var readAsString = (string)reader.Value;
                return TimeGuid.Parse(readAsString);
            }
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeGuid);
        }
    }
}