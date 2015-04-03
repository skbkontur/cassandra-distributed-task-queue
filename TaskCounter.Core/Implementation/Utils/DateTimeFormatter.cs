using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.Utils
{
    internal class DateTimeFormatter
    {
        public static string FormatWithMs(DateTime dateTime)
        {
            return dateTime.ToString(formatWithMs);
        }

        public static string FormatWithMsAndTicks(DateTime dateTime)
        {
            return string.Format("{0} (ticks={1})", dateTime.ToString(formatWithMs), dateTime.Ticks);
        }

        public static string FormatWithMsAndTicks(long ticks)
        {
            return FormatWithMsAndTicks(new DateTime(TicksToDateTimeRange(ticks), DateTimeKind.Utc));
        }

        public static string FormatTimeSpan(TimeSpan span)
        {
            return span.ToString("c") + " s";
        }

        public static long TicksToDateTimeRange(long ticks)
        {
            if (ticks < minTicks)
                ticks = minTicks;
            if (ticks > maxTicks)
                ticks = maxTicks;
            return ticks;
        }

        private const string formatWithMs = "dd.MM.yyyy HH:mm:ss.fff";

        private static readonly long minTicks = DateTime.MinValue.Ticks;
        private static readonly long maxTicks = DateTime.MaxValue.Ticks;
    }
}