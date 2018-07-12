using System;
using System.Globalization;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Cassandra.Repositories
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
            return string.Format("{0}_{1}", eventTicks.ToString("D20", CultureInfo.InvariantCulture), eventId);
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