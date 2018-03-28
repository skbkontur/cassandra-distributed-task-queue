using System;
using System.Text;

using JetBrains.Annotations;

using RemoteTaskQueue.Monitoring.Storage.Utils;

namespace RemoteTaskQueue.Monitoring.Storage.Search
{
    public class SearchIndexNameFactory
    {
        [NotNull]
        public static string GetIndexForTimeRange(long fromTicksUtc, long toTicksUtc)
        {
            var stringBuilder = new StringBuilder();
            var time = DateTimeFormatter.DateFromTicks(fromTicksUtc);
            var endTime = DateTimeFormatter.DateFromTicks(toTicksUtc).Add(minimumSupportedIndexCreationInterval);
            var dayWildcardFormat = RtqElasticsearchConsts.DataIndexNameFormat.Replace("dd", "*");
            var monthWildcardFormat = dayWildcardFormat.Replace("MM", "*");
            while(time < endTime)
            {
                var moved = false;
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
                    Append(stringBuilder, time, RtqElasticsearchConsts.DataIndexNameFormat);
                    time = time.AddDays(1);
                }
            }
            return stringBuilder.ToString();
        }

        private static void Append([NotNull] StringBuilder stringBuilder, DateTime time, [NotNull] string fmt)
        {
            if(stringBuilder.Length > 0)
                stringBuilder.Append(',');
            stringBuilder.Append(time.ToString(fmt));
        }

        private static readonly TimeSpan minimumSupportedIndexCreationInterval = TimeSpan.FromDays(1);
    }
}