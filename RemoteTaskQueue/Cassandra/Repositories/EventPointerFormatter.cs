using System;
using System.Globalization;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Objects;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories
{
    public static class EventPointerFormatter
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
        public static string GetMaxColumnNameForTimestamp([NotNull] Timestamp timestamp)
        {
            return GetColumnName(timestamp.Ticks, GuidHelpers.MaxGuid);
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
    }
}