using System.Web.Mvc;
using System.Web.Mvc.Html;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails.TaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.RenderingHelpers
{
    public static class TaskDataValueHtmlPrimitives
    {
        public static MvcHtmlString ObjectValue(this HtmlHelper htmlHelper, ObjectTaskDataModel model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/TaskData/_ObjectValue.cshtml", model);
        }

        public static MvcHtmlString StringValue(this HtmlHelper htmlHelper, StringTaskDataValue model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/TaskData/_StringValue.cshtml", model);
        }

        public static MvcHtmlString EmptyValue(this HtmlHelper htmlHelper, EmptyTaskDataValue model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/TaskData/_EmptyValue.cshtml", model);
        }

        public static MvcHtmlString ByteArrayValue(this HtmlHelper htmlHelper, ByteArrayTaskDataValue model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/TaskData/_ByteArrayValue.cshtml", model);
        }

        public static MvcHtmlString TaskState(this HtmlHelper htmlHelper, TaskMetaInfoModel model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/TaskData/_TaskState.cshtml", model);
        }

        public static MvcHtmlString FileDataTaskDataValue(this HtmlHelper htmlHelper, FileDataTaskDataValue model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/TaskData/_FileDataValue.cshtml", model);
        }
    }
}