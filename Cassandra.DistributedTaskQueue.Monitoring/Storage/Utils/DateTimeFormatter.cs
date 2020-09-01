using System;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Utils
{
    public static class DateTimeFormatter
    {
        public static DateTime DateFromTicks(long ticks)
        {
            return new DateTime(TicksToDateTimeRange(ticks), DateTimeKind.Utc).Date;
        }

        private static long TicksToDateTimeRange(long ticks)
        {
            if (ticks < minTicks)
                ticks = minTicks;
            if (ticks > maxTicks)
                ticks = maxTicks;
            return ticks;
        }

        private static readonly long minTicks = DateTime.MinValue.Ticks;
        private static readonly long maxTicks = DateTime.MaxValue.Ticks;
    }
}