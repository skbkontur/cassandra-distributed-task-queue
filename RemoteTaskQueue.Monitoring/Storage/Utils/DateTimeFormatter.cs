using System;

using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.Storage.Utils
{
    public static class DateTimeFormatter
    {
        public static string FormatWithMsAndTicks(long ticks)
        {
            return new Timestamp(ticks).ToString();
        }

        public static long TicksToDateTimeRange(long ticks)
        {
            if(ticks < minTicks)
                ticks = minTicks;
            if(ticks > maxTicks)
                ticks = maxTicks;
            return ticks;
        }

        private static readonly long minTicks = DateTime.MinValue.Ticks;
        private static readonly long maxTicks = DateTime.MaxValue.Ticks;
    }
}