﻿using System;

using Newtonsoft.Json;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    internal class LongStringsToNullConverter : JsonConverter
    {
        public LongStringsToNullConverter(int stringToNullThreshold)
        {
            this.stringToNullThreshold = stringToNullThreshold;
        }

        public override bool CanRead { get { return false; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var str = value as string;
            if(str == null || str.Length <= stringToNullThreshold)
                writer.WriteValue(str);
            else
                writer.WriteNull();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        private readonly int stringToNullThreshold;
    }
}