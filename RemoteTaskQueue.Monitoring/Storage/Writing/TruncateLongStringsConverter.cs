using System;

using Newtonsoft.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing
{
    public class TruncateLongStringsConverter : JsonConverter
    {
        public TruncateLongStringsConverter(int maxStringLength)
        {
            this.maxStringLength = maxStringLength;
        }

        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var str = value as string;
            if (str == null || str.Length <= maxStringLength)
                writer.WriteValue(str);
            else
                writer.WriteValue(str.Substring(0, maxStringLength));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Operation is not supported");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        private readonly int maxStringLength;
    }
}