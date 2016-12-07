using System;

using Newtonsoft.Json;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    internal class TruncateLongStringsConverter : JsonConverter
    {
        public TruncateLongStringsConverter(int maxStringLength)
        {
            this.maxStringLength = maxStringLength;
        }

        public override bool CanRead { get { return false; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var str = value as string;
            if(str == null || str.Length <= maxStringLength)
                writer.WriteValue(str);
            else
                writer.WriteValue(str.Substring(0, maxStringLength));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new InvalidProgramStateException("Operation is not supported");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        private readonly int maxStringLength;
    }
}