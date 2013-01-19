using System;
using System.Web.Mvc;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.RenderingHelpers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails.TaskData
{
    public class ByteArrayTaskDataValue : ITaskDataValue
    {
        public MvcHtmlString Render(HtmlHelper htmlHelper)
        {
            return htmlHelper.ByteArrayValue(this);
        }

        public int Size { get; set; }
        public Func<UrlHelper, string> GetUrl { get; set; }
    }
}