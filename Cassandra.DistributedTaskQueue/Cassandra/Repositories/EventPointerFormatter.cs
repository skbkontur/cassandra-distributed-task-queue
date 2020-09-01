using System;
using System.Globalization;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories
{
    internal static class EventPointerFormatter
    {
        [NotNull]
        public static string GetPartitionKey(long eventTicks)
        {
            return (eventTicks / PartitionDurationTicks).ToString();
        }

        public static long ParsePartitionKey([NotNull] string partitionKey)
        {
            return long.Parse(partitionKey);
        }

        [NotNull]
        public static string GetColumnName(long eventTicks, Guid eventId)
        {
            return $"{eventTicks.ToString("D20", CultureInfo.InvariantCulture)}_{eventId}";
        }

        [NotNull]
        public static string GetMaxColumnNameForTicks(long eventTicks)
        {
            return GetColumnName(eventTicks, MaxGuid);
        }

        [NotNull]
        public static string GetMaxColumnNameForTimestamp([NotNull] Timestamp timestamp)
        {
            return GetMaxColumnNameForTicks(timestamp.Ticks);
        }

        [NotNull]
        public static Timestamp GetTimestamp([NotNull] string eventColumnName)
        {
            return new Timestamp(long.Parse(eventColumnName.Split('_')[0]));
        }

        public static Guid GetEventId([NotNull] string eventColumnName)
        {
            return Guid.Parse(eventColumnName.Split('_')[1]);
        }

        public static int CompareColumnNames([NotNull] string x, [NotNull] string y)
        {
            return string.Compare(x, y, StringComparison.Ordinal); // compare as cassandra string column names
        }

        public static readonly long PartitionDurationTicks = TimeSpan.FromMinutes(6).Ticks;
        public static readonly Guid MaxGuid = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
    }
}