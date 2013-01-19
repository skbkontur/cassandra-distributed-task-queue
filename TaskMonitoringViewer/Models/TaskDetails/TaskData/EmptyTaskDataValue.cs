using System.Web.Mvc;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.RenderingHelpers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails.TaskData
{
    public class EmptyTaskDataValue : ITaskDataValue
    {
        public MvcHtmlString Render(HtmlHelper htmlHelper)
        {
            return htmlHelper.EmptyValue(this);
        }
    }
}