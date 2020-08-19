using System;

using Newtonsoft.Json;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json
{
    public class TimestampJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
                writer.WriteValue(((Timestamp)value).ToDateTime());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
            case JsonToken.Null:
                return null;
            case JsonToken.Date:
                return new Timestamp((DateTime)reader.Value);
            case JsonToken.Integer:
                return new Timestamp((long)reader.Value);
            }
            throw new JsonSerializationException($"Unexpected token when parsing timestamp. Expected Date or Integer with value type long, got {reader.TokenType}");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Timestamp);
        }
    }
}