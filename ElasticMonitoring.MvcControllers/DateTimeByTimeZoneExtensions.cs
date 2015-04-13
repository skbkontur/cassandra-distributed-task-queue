using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers
{
    internal static class DateTimeByTimeZoneExtensions
    {
        public static DateTime? ToMoscowTime(this DateTime? utc)
        {
            if(!utc.HasValue)
                return null;
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utc.Value, moscowTimeZone.Id);
        }

        public static string ToFullMoscowTimeString(this DateTime? utc)
        {
            var moscowTime = utc.ToMoscowTime();
            if(!moscowTime.HasValue)
                return "";
            return moscowTime.Value.ToString("dd.MM.yyyy HH:mm:ss");
        }

        private static TimeZoneInfo GetMoscowTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            }
            catch(Exception e)
            {
                return TimeZoneInfo.Local;
            }
        }

        private static readonly TimeZoneInfo moscowTimeZone = GetMoscowTimeZone();
    }
}