using System;
using System.Globalization;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public static class CassandraNameHelper
    {
        [NotNull]
        public static string GetRowKey([NotNull] TaskIndexShardKey taskIndexShardKey, long ticks)
        {
            return string.Format("{0}_{1}", GetTicksRowNumber(ticks), taskIndexShardKey.ToCassandraKey());
        }

        [NotNull]
        public static string ToCassandraKey([NotNull] this TaskIndexShardKey taskIndexShardKey)
        {
            var taskStateCassandraName = taskIndexShardKey.TaskState.GetCassandraName();
            return string.Format("{0}_{1}", taskIndexShardKey.TaskTopic, taskStateCassandraName);
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
            return ticks / TaskMinimalStartTicksIndexTicksPartition;
        }

        public static long GetMinimalTicksForRow(long rowNumber)
        {
            return rowNumber * TaskMinimalStartTicksIndexTicksPartition;
        }

        public static readonly long TaskMinimalStartTicksIndexTicksPartition = TimeSpan.FromMinutes(6).Ticks;
    }
}