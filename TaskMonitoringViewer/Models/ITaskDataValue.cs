using System.Web.Mvc;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models
{
    public interface ITaskDataValue
    {
        MvcHtmlString Render(HtmlHelper htmlHelper);
    }
}