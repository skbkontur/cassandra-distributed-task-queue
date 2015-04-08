using System;
using System.Text;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage
{
    public static class IndexNameFactory
    {
        public static string GetIndexForTime(long ticksUtc)
        {
            //NOTE CANNOT rename already created indices
            return BuildIndexNameForTime(TaskSearchIndexSchema.IndexPrefix, ticksUtc);
        }

        public static void GetDateRange(long ticksUtc, out DateTime beginDateInc, out DateTime endDateExc)
        {
            var dateTime = DateFromTicks(ticksUtc);
            beginDateInc = DateToBeginDate(dateTime);
            endDateExc = DateToEndDate(dateTime);
        }

        public static string BuildIndexNameForTime(string prefix, long ticksUtc)
        {
            return prefix + ticksUtc.DateFromTicks().DateToBeginDate().DateToString();
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
                stringBuilder.Append(time.DateToBeginDate().DateToString());
                time = time.Add(newIndexCreationInterval);
            }
            return stringBuilder.ToString();
        }

        private static DateTime DateToEndDate(this DateTime dateTime)
        {
            return dateTime.Add(newIndexCreationInterval).Date;
        }

        private static DateTime DateToBeginDate(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        private static DateTime DateFromTicks(this long ticks)
        {
            return new DateTime(DateTimeFormatter.TicksToDateTimeRange(ticks), DateTimeKind.Utc);
        }

        private static string DateToString(this DateTime dateTime)
        {
            return dateTime.ToString(dateFormat);
        }

        private const string dateFormat = "yyyy.MM.dd";

        private static readonly TimeSpan newIndexCreationInterval = TimeSpan.FromDays(1);
    }
}