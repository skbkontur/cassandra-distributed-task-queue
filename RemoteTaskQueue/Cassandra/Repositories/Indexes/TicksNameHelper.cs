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
            return (ticks / tickPartition) + "_" + taskState.GetCassandraName();
        }

        [NotNull]
        private static string GetColumnName(long ticks, [NotNull] string suffix)
        {
            return ticks.ToString("D20", CultureInfo.InvariantCulture) + "_" + suffix;
        }

        public static long GetTicksFromColumnName([NotNull] string columnName)
        {
            var ticksString = columnName.Split('_')[0];
            long res;
            if(!long.TryParse(ticksString, out res))
                return 0;
            return res;
        }

        public static long GetTicksRowNumber(long ticks)
        {
            return ticks / tickPartition;
        }

        public static long GetMinimalTicksForRow(long rowNumber)
        {
            return rowNumber * tickPartition;
        }

        [NotNull]
        public static TaskColumnInfo GetColumnInfo([NotNull] TaskMetaInformation taskMeta)
        {
            var rowKey = GetRowKey(taskMeta.State, taskMeta.MinimalStartTicks);
            var columnName = GetColumnName(taskMeta.MinimalStartTicks, taskMeta.Id);
            return new TaskColumnInfo(rowKey, columnName);
        }

        [NotNull]
        public static TaskColumnInfo GetColumnInfo(TaskState taskState, long ticks, [NotNull] string colSuffix)
        {
            var rowKey = GetRowKey(taskState, ticks);
            var columnName = GetColumnName(ticks, colSuffix);
            return new TaskColumnInfo(rowKey, columnName);
        }

        private static readonly long tickPartition = TimeSpan.FromMinutes(6).Ticks;
    }
}