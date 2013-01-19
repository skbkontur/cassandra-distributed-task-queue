using System.Web.Mvc;
using System.Web.Mvc.Html;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.RenderingHelpers
{
    public static class HtmlPrimtives
    {
        public static MvcHtmlString TaskState(this HtmlHelper htmlHelper, TaskStateHtmlModel model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/_TaskState.cshtml", model);
        }

        public static MvcHtmlString TaskId(this HtmlHelper htmlHelper, TaskIdHtmlModel model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/_TaskId.cshtml", model);
        }

        public static MvcHtmlString TaskDateTime(this HtmlHelper htmlHelper, TaskDateTimeHtmlModel model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/_TaskDateTime.cshtml", model);
        }

        public static MvcHtmlString ExceptionInfo(this HtmlHelper htmlHelper, ExceptionInfoHtmlModel model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/_ExceptionInfo.cshtml", model);
        }
    }
}