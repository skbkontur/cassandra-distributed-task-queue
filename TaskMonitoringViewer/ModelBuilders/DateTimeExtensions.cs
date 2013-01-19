using System;
using System.Globalization;

using log4net;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    internal static class DateTimeExtensions
    {
        public static DateTime? UtcToMoscowDateTime(this DateTime? utc)
        {
            return utc.HasValue ? (DateTime?)utc.Value.ToMoscowDateTime() : null;
        }

        public static DateTime? MoscowToUtcDateTime(this DateTime? moscowDateTime)
        {
            if(moscowDateTime.HasValue)
                return moscowDateTime.Value.ToUtcDateTime();
            return null;
        }

        public static string GetMoscowDateTimeString(this DateTime? utc)
        {
            return utc == null ? null : utc.Value.GetMoscowDateTimeString();
        }

        private static string GetMoscowDateTimeString(this DateTime utc)
        {
            return utc.GetMoscowDateString() + " " + utc.GetMoscowTimeString();
        }

        private static DateTime ToUtcDateTime(this DateTime moscowDateTime)
        {
            var res = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(moscowDateTime, moscowTimeZone.Id, TimeZoneInfo.Utc.Id);
            return res;
        }

        private static DateTime ToMoscowDateTime(this DateTime utc)
        {
            var res = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utc, TimeZoneInfo.Utc.Id, moscowTimeZone.Id);
            return res;
        }

        private static string GetMoscowDateString(this DateTime utc)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utc, moscowTimeZone.Id).ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("ru"));
        }

        private static string GetMoscowTimeString(this DateTime utc)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utc, moscowTimeZone.Id).ToString("HH:mm:ss");
        }

        private static TimeZoneInfo GetMoscowTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            }
            catch(Exception e)
            {
                logger.Warn("Не смогли найти TimeZoneInfo для Москвы. Используем локальное время сервера с Id = " + TimeZoneInfo.Local.Id, e);
                return TimeZoneInfo.Local;
            }
        }

        private static readonly ILog logger = LogManager.GetLogger(typeof(DateTimeExtensions));
        private static readonly TimeZoneInfo moscowTimeZone = GetMoscowTimeZone();
    }
}