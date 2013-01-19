using System.Web.Mvc;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.RenderingHelpers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails.TaskData
{
    public class StringTaskDataValue : ITaskDataValue
    {
        public MvcHtmlString Render(HtmlHelper htmlHelper)
        {
            return htmlHelper.StringValue(this);
        }

        public string Value { get; set; }
    }
}