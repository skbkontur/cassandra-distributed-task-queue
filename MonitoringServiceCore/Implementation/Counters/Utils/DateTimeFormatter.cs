using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters.Utils
{
    public class DateTimeFormatter
    {
        public static string FormatWithMs(DateTime dateTime)
        {
            return dateTime.ToString(formatWithMs);
        }

        public static string FormatWithMsAndTicks(DateTime dateTime)
        {
            return string.Format("{0} (ticks={1})", dateTime.ToString(formatWithMs), dateTime.Ticks);
        }

        public static string FormatWithMsAndTicks(long ticksUtc)
        {
            return FormatWithMsAndTicks(new DateTime(ticksUtc, DateTimeKind.Utc));
        }

        private const string formatWithMs = "dd.MM.yyyy HH:mm:ss.fff";
    }
}