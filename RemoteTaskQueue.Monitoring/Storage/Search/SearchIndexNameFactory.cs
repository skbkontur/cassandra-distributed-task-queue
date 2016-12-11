using System;
using System.Text;

using RemoteTaskQueue.Monitoring.Storage.Utils;

namespace RemoteTaskQueue.Monitoring.Storage.Search
{
    public class SearchIndexNameFactory
    {
        public static string GetIndexForTimeRange(long fromTicksUtc, long toTicksUtc)
        {
            return GetIndexForTimeRange(fromTicksUtc, toTicksUtc, searchIndexNameFormat);
        }

        public static string GetIndexForTimeRange(long fromTicksUtc, long toTicksUtc, string indexNameFormat)
        {
            var stringBuilder = new StringBuilder();
            var time = DateToBeginDate(DateFromTicks(fromTicksUtc));
            var endTime = DateToBeginDate(DateFromTicks(toTicksUtc)).Add(minimumSupportedIndexCreationInterval);
            var dayWildcardFormat = indexNameFormat.Replace("dd", "*"); //todo yyyy.MM.*
            var monthWildcardFormat = dayWildcardFormat.Replace("MM", "*"); //todo yyyy.*.*
            while(time < endTime)
            {
                bool moved = false;
                if(time.Day == 1)
                {
                    DateTime nextTime;
                    if(time.Month == 1)
                    {
                        if((nextTime = time.AddYears(1)) <= endTime)
                        {
                            Append(stringBuilder, time, monthWildcardFormat);
                            time = nextTime;
                            moved = true;
                        }
                    }
                    if(!moved && (nextTime = time.AddMonths(1)) <= endTime)
                    {
                        Append(stringBuilder, time, dayWildcardFormat);
                        time = nextTime;
                        moved = true;
                    }
                }
                if(!moved)
                {
                    Append(stringBuilder, time, indexNameFormat);
                    time = time.AddDays(1);
                }
            }
            return stringBuilder.ToString();
        }

        private static void Append(StringBuilder stringBuilder, DateTime time, string fmt)
        {
            if(stringBuilder.Length > 0)
                stringBuilder.Append(',');
            stringBuilder.Append(time.ToString(fmt));
        }

        private static DateTime DateToBeginDate(DateTime dateTime)
        {
            return dateTime.Date;
        }

        private static DateTime DateFromTicks(long ticks)
        {
            return new DateTime(DateTimeFormatter.TicksToDateTimeRange(ticks), DateTimeKind.Utc);
        }

        private static readonly TimeSpan minimumSupportedIndexCreationInterval = TimeSpan.FromDays(1);
        private static readonly string searchIndexNameFormat = IndexNameConverter.ConvertToDateTimeFormat(IndexNameConverter.FillIndexNamePlaceholder(RtqElasticsearchConsts.SearchAliasFormat, RtqElasticsearchConsts.CurrentIndexNameFormat));
    }
}