using System;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    public class WriteIndexNameFactory : IWriteIndexNameFactory
    {
        public WriteIndexNameFactory(ITaskWriteDynamicSettings settings, IndexChecker indexChecker)
        {
            currentFormat = settings.CurrentIndexNameFormat;
            oldFormat = settings.OldIndexNameFormat;
            this.indexChecker = indexChecker;
        }

        public string GetIndexForTask(TaskMetaInformation taskMetaInformation)
        {
            if(IsOldTask(taskMetaInformation))
            {
                var indexName = BuildIndexNameForTime(taskMetaInformation.Ticks, oldFormat);
                if(oldFormat != currentFormat && !indexChecker.CheckAliasExists(indexName))
                    indexName = BuildIndexNameForTime(taskMetaInformation.Ticks, currentFormat);
                return indexName;
            }
            return BuildIndexNameForTime(taskMetaInformation.Ticks, currentFormat);
        }

        private bool IsOldTask(TaskMetaInformation taskMetaInformation)
        {
            return taskMetaInformation.LastModificationTicks.Value > oldIntervalTicks + taskMetaInformation.Ticks;
        }

        private static string BuildIndexNameForTime(long ticksUtc, string format)
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

        public static readonly TimeSpan OldTaskInterval = TimeSpan.FromDays(1); //todo move it

        private readonly long oldIntervalTicks = OldTaskInterval.Ticks;

        private readonly IndexChecker indexChecker;
        private readonly string oldFormat;
        private readonly string currentFormat;
    }
}