using System.Web.Mvc;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.RenderingHelpers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails.TaskData
{
    public class ObjectTaskDataModel : ITaskDataValue
    {
        public MvcHtmlString Render(HtmlHelper htmlHelper)
        {
            return htmlHelper.ObjectValue(this);
        }

        public TaskDataProperty[] Properties { get; set; }

        public class TaskDataProperty
        {
            public string Name { get; set; }
            public ITaskDataValue Value { get; set; }
            public bool Hidden { get; set; }
        }
    }
}