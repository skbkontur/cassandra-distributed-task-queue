using System;
using System.Text;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Search
{
    public class SearchIndexNameFactory
    {
        public SearchIndexNameFactory(TaskSearchDynamicSettings settings)
        {
            searchIndexNameFormat = settings.SearchIndexNameFormat;
        }

        public string GetIndexForTimeRange(long fromTicksUtc, long toTicksUtc)
        {
            //todo now output is big for large interval. use wildcards??
            var stringBuilder = new StringBuilder();
            var time = DateFromTicks(fromTicksUtc);
            var endTime = DateFromTicks(toTicksUtc);
            string lastName = null;
            while(time <= endTime)
            {
                var currentName = DateToBeginDate(time).ToString(searchIndexNameFormat);
                if(lastName != currentName)
                {
                    lastName = currentName;
                    if(stringBuilder.Length > 0)
                        stringBuilder.Append(',');
                    stringBuilder.Append(currentName);
                }
                time = time.Add(minimumSupportedIndexCreationInterval);
            }
            return stringBuilder.ToString();
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
        private readonly string searchIndexNameFormat;
    }
}