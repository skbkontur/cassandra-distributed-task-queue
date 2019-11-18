using System;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RemoteTaskQueue.Monitoring.Storage.Utils
{
    public class UtcTicksJsonConverter : IsoDateTimeConverter
    {
        public UtcTicksJsonConverter()
        {
            Culture = CultureInfo.InvariantCulture;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(long) || base.CanConvert(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var underlyingType = Nullable.GetUnderlyingType(objectType);
            var type = underlyingType ?? objectType;
            if (type == typeof(long))
            {
                var res = base.ReadJson(reader, typeof(DateTime?), existingValue, serializer);
                if (res == null)
                {
                    if (underlyingType != null)
                        return null;
                    return 0L;
                }
                return ((DateTime)res).Ticks;
            }
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is long)
                base.WriteJson(writer, CreateDateTime((long)value), serializer);
            else
                base.WriteJson(writer, value, serializer);
        }

        private static DateTime CreateDateTime(long value)
        {
            if (value < minTicks)
                value = minTicks;
            if (value > maxTicks)
                value = maxTicks;
            return new DateTime(value, DateTimeKind.Utc);
        }

        private static readonly long minTicks = DateTime.MinValue.Ticks;
        private static readonly long maxTicks = DateTime.MaxValue.Ticks;
    }
}