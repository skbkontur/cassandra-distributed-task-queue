using System;
using System.Globalization;

using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes
{
    public static class TicksNameHelper
    {
        [NotNull]
        public static string GetRowKey([NotNull] TaskTopicAndState taskTopicAndState, long ticks)
        {
            return string.Format("{0}_{1}", GetTicksRowNumber(ticks), taskTopicAndState.ToCassandraKey());
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

        private static readonly long ticksPartition = TimeSpan.FromMinutes(6).Ticks;
    }
}