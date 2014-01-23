using System;

using SKBKontur.Catalogue.WebTestCore.SystemControls;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.Controls
{
    public class DateAndTimeHtmlControl : HtmlControl
    {
        public DateAndTimeHtmlControl(string id)
            : base(id)
        {
            Date = new StaticText(string.Format("{0}_Date", id), this);
            Time = new StaticText(string.Format("{0}_Time", id), this);
            TimeZoneId = new StaticText(string.Format("{0}_timeZoneId", id), this);

        }

        public DateTime? GetDateTimeUtc()
        {
            var date = Date.GetText();
            var partsDate = date.Split('.');
            if(partsDate.Length != 3)
                return null;
            int year, mounth, day;
            if (!Int32.TryParse(partsDate[0], out day) || !Int32.TryParse(partsDate[1], out mounth) || !Int32.TryParse(partsDate[2], out year))
                return null;

            var time = Time.GetText();
            var partsTime = time.Split(':');
            if(partsTime.Length != 3)
                return null;
            int hours, minutes, seconds;
            if(!Int32.TryParse(partsTime[0], out hours) || !Int32.TryParse(partsTime[1], out minutes) || !Int32.TryParse(partsTime[2], out seconds))
                return null;
            
            var timeZoneId = TimeZoneId.GetText();
            if(string.IsNullOrEmpty(timeZoneId))
                return null;

            return TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, mounth, day, hours, minutes, seconds, DateTimeKind.Unspecified), TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        }

        public StaticText Date { get; set; }
        public StaticText Time { get; set; }
        public StaticText TimeZoneId { get; set; }

    }
}