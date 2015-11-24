using System;
using System.Globalization;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes
{
    public static class TicksNameHelper
    {
        public static string GetRowName(TaskState taskState, long ticks)
        {
            return (ticks / tickPartition) + "_" + taskState.GetCassandraName();
        }

        public static string GetColumnName(long ticks, string suffix)
        {
            return ticks.ToString("D20", CultureInfo.InvariantCulture) + "_" + suffix;
        }

        public static string GetRowName(TaskMetaInformation taskMetaInformation)
        {
            if(taskMetaInformation == null) return null;
            return GetRowName(taskMetaInformation.State, taskMetaInformation.MinimalStartTicks);
        }

        public static string GetColumnName(TaskMetaInformation taskMetaInformation)
        {
            if(taskMetaInformation == null) return null;
            return GetColumnName(taskMetaInformation.MinimalStartTicks, taskMetaInformation.Id);
        }

        public static long GetTicksFromColumnName(string columnName)
        {
            string ticksString = columnName.Split('_')[0];
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

        public static ColumnInfo GetColumnInfo(TaskMetaInformation taskMetaInformation)
        {
            if(taskMetaInformation == null) return null;
            return new ColumnInfo
                {
                    RowKey = GetRowName(taskMetaInformation),
                    ColumnName = GetColumnName(taskMetaInformation)
                };
        }

        public static ColumnInfo GetColumnInfo(TaskState taskState, long ticks, string colSuffix)
        {
            string rowKey = GetRowName(taskState, ticks);
            string columnName = GetColumnName(ticks, colSuffix);
            return new ColumnInfo
                {
                    RowKey = rowKey,
                    ColumnName = columnName
                };
        }

        private static readonly long tickPartition = TimeSpan.FromMinutes(6).Ticks;
    }
}