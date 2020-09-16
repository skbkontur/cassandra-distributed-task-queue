using System;

using Newtonsoft.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json
{
    /// <summary>
    ///     Конвертер, которые сериализует long-и в строки вместо чисел,
    ///     ибо JSON.parse() на длинных long-ах теряет последние разряды
    /// </summary>
    public class LongToStringConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(value.ToString());
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<string>(reader);
            if (value == null)
            {
                return null;
            }
            return long.Parse(value);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(long) == objectType || typeof(long?) == objectType;
        }
    }
}