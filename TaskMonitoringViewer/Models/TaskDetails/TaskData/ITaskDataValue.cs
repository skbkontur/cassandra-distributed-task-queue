using System.Web.Mvc;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails.TaskData
{
    public interface ITaskDataValue
    {
        MvcHtmlString Render(HtmlHelper htmlHelper);
    }
}