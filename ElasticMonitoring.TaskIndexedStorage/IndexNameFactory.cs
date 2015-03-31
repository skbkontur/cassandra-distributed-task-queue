using System;
using System.Text;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage
{
    public class IndexNameFactory
    {
        public static string GetIndexForTime(long ticksUtc)
        {
            //NOTE CANNOT rename already created indices
            return TaskSearchIndexSchema.IndexPrefix + FormatTimeToIndexName(DateFromTicks(ticksUtc));
        }

        public static string GetIndexForTimeRange(long fromTicksUtc, long toTicksUtc)
        {
            //todo now output is big for large interval. use wildcards??
            var stringBuilder = new StringBuilder();
            var time = DateFromTicks(fromTicksUtc);
            var endTime = DateFromTicks(toTicksUtc);
            while(time <= endTime)
            {
                if(stringBuilder.Length > 0)
                    stringBuilder.Append(',');
                stringBuilder.Append(TaskSearchIndexSchema.IndexPrefix);
                stringBuilder.Append(FormatTimeToIndexName(time));
                time = time.Add(newIndexCreationInterval);
            }
            return stringBuilder.ToString();
        }

        private static string FormatTimeToIndexName(DateTime dateTime)
        {
            return dateTime.ToString(dateFormat);
        }

        private static DateTime DateFromTicks(long ticks)
        {
            return new DateTime(DateTimeFormatter.TicksToDateTimeRange(ticks), DateTimeKind.Utc);
        }

        private const string dateFormat = "yyyy.MM.dd";

        private static readonly TimeSpan newIndexCreationInterval = TimeSpan.FromDays(1);
    }
}