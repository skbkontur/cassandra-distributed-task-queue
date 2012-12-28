using System;
using System.Globalization;

using log4net;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public static class DateTimeExtensions
    {
        public static string GetDateString(this DateTime? dateTime, string emptyValue = null)
        {
            if (dateTime == null)
                return emptyValue;
            return string.Format("{0:dd.MM.yyyy}", dateTime.Value);
        }

        public static string GetMoscowDateString(this DateTime? utc, string emptyValue = null)
        {
            if (!utc.HasValue) return emptyValue;
            return utc.Value.GetMoscowDateString();
        }

        public static string GetMoscowTimeString(this DateTime? utc, string emptyValue = null)
        {
            if (!utc.HasValue) return emptyValue;
            return utc.Value.GetMoscowTimeString();
        }

        public static string GetMoscowDateString(this DateTime utc)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utc, moscowTimeZone.Id).ToString("dd MMMM yyyy", CultureInfo.GetCultureInfo("ru"));
        }

        public static string GetMoscowTimeString(this DateTime utc)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utc, moscowTimeZone.Id).ToString("HH:mm");
        }

        public static string GetMoscowDateTimeString(this DateTime utc)
        {
            return utc.GetMoscowDateString() + " " + utc.GetMoscowTimeString();
        }

        private static TimeZoneInfo GetMoscowTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            }
            catch (Exception e)
            {
                logger.Warn("Не смогли найти TimeZoneInfo для Москвы. Используем локальное время сервера с Id = " + TimeZoneInfo.Local.Id, e);
                return TimeZoneInfo.Local;
            }
        }

        private static readonly ILog logger = LogManager.GetLogger(typeof(DateTimeExtensions));
        private static readonly TimeZoneInfo moscowTimeZone = GetMoscowTimeZone();
    }
}