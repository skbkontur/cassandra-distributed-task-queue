using System;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    public class WriteIndexNameFactory : IWriteIndexNameFactory
    {
        public WriteIndexNameFactory(IndexChecker indexChecker)
        {
            this.indexChecker = indexChecker;
        }

        public string GetIndexForTask(TaskMetaInformation taskMetaInformation)
        {
            if(IsOldTask(taskMetaInformation))
            {
                var indexName = BuildIndexNameForTime(taskMetaInformation.Ticks, oldIndexNameFormat);
                if(oldIndexNameFormat != CurrentIndexNameFormat && !indexChecker.CheckAliasExists(indexName))
                {
                    //NOTE эта ситуация возможна если пишут сразу старую задачу в не созданный еще индекс - alias еще не создан (тк индекс еще не создали)
                    indexName = BuildIndexNameForTime(taskMetaInformation.Ticks, CurrentIndexNameFormat);
                }
                return indexName;
            }
            return BuildIndexNameForTime(taskMetaInformation.Ticks, CurrentIndexNameFormat);
        }

        private static bool IsOldTask(TaskMetaInformation taskMetaInformation)
        {
            return taskMetaInformation.LastModificationTicks.Value > OldTaskInterval.Ticks + taskMetaInformation.Ticks;
        }

        public static string BuildIndexNameForTime(long ticksUtc, string format)
        {
            return DateToBeginDate(DateFromTicks(ticksUtc)).ToString(format);
        }

        private static DateTime DateToBeginDate(DateTime dateTime)
        {
            return dateTime.Date;
        }

        private static DateTime DateFromTicks(long ticks)
        {
            return new DateTime(DateTimeFormatter.TicksToDateTimeRange(ticks), DateTimeKind.Utc);
        }

        private readonly IndexChecker indexChecker;
        public static readonly TimeSpan OldTaskInterval = TimeSpan.FromDays(1);
        public static readonly string CurrentIndexNameFormat = IndexNameConverter.ConvertToDateTimeFormat(RtqElasticsearchConsts.CurrentIndexNameFormat);
        private static readonly string oldIndexNameFormat = IndexNameConverter.ConvertToDateTimeFormat(IndexNameConverter.FillIndexNamePlaceholder(RtqElasticsearchConsts.OldDataAliasFormat, RtqElasticsearchConsts.CurrentIndexNameFormat));
    }
}