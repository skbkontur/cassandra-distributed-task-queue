using System;
using System.Linq;
using System.Web.Mvc;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.AccessControl.AccessRules;
using SKBKontur.Catalogue.CassandraStorageCore.BusinessObjectStorageImpl;
using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Constants;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    [ResourseGroup(ResourseGroups.AdminResourse)]
    public abstract class RemoteTaskQueueControllerBase : AuthanticatedControllerBase
    {
        protected RemoteTaskQueueControllerBase(RemoteTaskQueueControllerBaseParameters remoteTaskQueueControllerBaseParameters)
            : base(remoteTaskQueueControllerBaseParameters.LoggedInControllerBaseParameters)
        {
            taskMetaInfoModelBuilder = remoteTaskQueueControllerBaseParameters.TaskMetaInfoModelBuilder;
            taskViewModelBuilder = remoteTaskQueueControllerBaseParameters.TaskViewModelBuilder;
            objectValueExtracter = remoteTaskQueueControllerBaseParameters.ObjectValueExtracter;
            monitoringServiceClient = remoteTaskQueueControllerBaseParameters.MonitoringServiceClient;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Run(int page = 0)
        {
            var countPerPage = ControllerConstants.DefaultRecordsNumberPerPage;
            var countTasks = monitoringServiceClient.GetCount();
            var totalPages = (countTasks + countPerPage - 1) / countPerPage;
            if(page < 0)
                page = 0;
            if(page >= totalPages)
                page = totalPages - 1;
            var taskMetas = monitoringServiceClient.GetRange(countPerPage * page, countPerPage);
            return View("RemoteTaskQueueListView", new TaskListModel
                {
                    PageNumber = page,
                    TotalPagesCount = totalPages,
                    PagesWindowSize = 3,
                    TaskModels = taskMetas.Select(taskMetaInfoModelBuilder.Build).ToArray()
                });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Show(string id)
        {
            var remoteTaskInfo = monitoringServiceClient.GetTaskInfo(id);
            return View("RemoteTaskQueueDetailsView", taskViewModelBuilder.Build(remoteTaskInfo));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Cancel(string id)
        {
            if(!monitoringServiceClient.CancelTask(id))
                return ReturnOperationResult(new UnsuccessOperationResult {ErrorMessage = "Задача не может быть отменена"});
            return ReturnOperationResult(new SuccessOperationResult
                {
                    ActionDescription = "Задача успешно отменена",
                    NeedRedirect = true,
                    RedirectTo = Request.Headers["Referer"]
                });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Rerun(string id)
        {
            //if(!remoteTaskQueue.RerunTask(id, TimeSpan.FromTicks(0)))
            //    return ReturnOperationResult(new UnsuccessOperationResult {ErrorMessage = "Задача не может перезапущена"});
            return ReturnOperationResult(new SuccessOperationResult
                {
                    ActionDescription = "Задача успешно перезапущена",
                    NeedRedirect = true,
                    RedirectTo = Request.Headers["Referer"]
                });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetBytes(string id, string path)
        {
            var taskData = monitoringServiceClient.GetTaskInfo(id).TaskData;
            var value = objectValueExtracter.Extract(taskData.GetType(), taskData, path);
            if(value.GetType() != typeof(byte[]))
                throw new Exception(string.Format("Type of property by path '{0}' has type '{1}' instead of '{2}'", path, value.GetType(), typeof(byte[])));
            var fileDownloadName = string.Format("{0}_{1}.xml", DateTime.UtcNow.ToString("yyyy.MM.dd hh:mm:ss"), Guid.NewGuid());
            return File((byte[])value, "application/xml", fileDownloadName);
        }

        private static TaskState[] GetAllTaskStates()
        {
            return Enum.GetValues(typeof(TaskState)).Cast<TaskState>().ToArray();
        }

        private readonly ITaskMetaInfoModelBuilder taskMetaInfoModelBuilder;
        private readonly ITaskViewModelBuilder taskViewModelBuilder;
        private readonly IObjectValueExtracter objectValueExtracter;
        private readonly IMonitoringServiceClient monitoringServiceClient;
    }
}