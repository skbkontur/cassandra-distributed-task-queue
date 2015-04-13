using System.Web.Mvc;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.TaskDetailsTreeView
{
    internal class TaskDataBuildingContext
    {
        public string TaskId { get; set; }
        public UrlHelper UrlHelper { get; set; }
    }
}