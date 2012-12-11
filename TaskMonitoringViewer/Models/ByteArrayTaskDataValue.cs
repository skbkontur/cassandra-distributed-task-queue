using System.Web.Mvc;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.RenderingHelpers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models
{
    public class ByteArrayTaskDataValue : ITaskDataValue
    {
        public MvcHtmlString Render(HtmlHelper htmlHelper)
        {
            return htmlHelper.ByteArrayValue(this);
        }

        public int Size { get; set; }
        public string TaskId { get; set; }
        public string Path { get; set; }
    }
}