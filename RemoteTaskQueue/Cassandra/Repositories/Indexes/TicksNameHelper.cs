using System;
using System.Globalization;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes
{
    public static class TicksNameHelper
    {
        [NotNull]
        public static string GetRowKey(TaskState taskState, long ticks)
        {
            return string.Format("{0}_{1}", GetTicksRowNumber(ticks), taskState.GetCassandraName());
        }

        [NotNull]
        public static string GetColumnName(long ticks, [NotNull] string suffix)
        {
            return string.Format("{0}_{1}", ticks.ToString("D20", CultureInfo.InvariantCulture), suffix);
        }

        public static long GetTicksFromColumnName([NotNull] string columnName)
        {
            return long.Parse(columnName.Split('_')[0]);
        }

        public static long GetTicksRowNumber(long ticks)
        {
            return ticks / ticksPartition;
        }

        public static long GetMinimalTicksForRow(long rowNumber)
        {
            return rowNumber * ticksPartition;
        }

        [NotNull]
        public static TaskColumnInfo GetColumnInfo([NotNull] TaskMetaInformation taskMeta)
        {
            var rowKey = GetRowKey(taskMeta.State, taskMeta.MinimalStartTicks);
            var columnName = GetColumnName(taskMeta.MinimalStartTicks, taskMeta.Id);
            return new TaskColumnInfo(rowKey, columnName);
        }

        private static readonly long ticksPartition = TimeSpan.FromMinutes(6).Ticks;
    }
}