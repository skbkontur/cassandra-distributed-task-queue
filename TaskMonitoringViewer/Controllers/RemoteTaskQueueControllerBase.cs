using System;
using System.Linq;
using System.Web.Mvc;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.AccessControl.AccessRules;
using SKBKontur.Catalogue.CassandraStorageCore.BusinessObjectStorageImpl;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.Core.Web.Models.DateAndTimeModels;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.Html;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    [ResourseGroup(ResourseGroups.AdminResourse)]
    public abstract class RemoteTaskQueueControllerBase : AuthanticatedControllerBase
    {
        protected RemoteTaskQueueControllerBase(RemoteTaskQueueControllerBaseParameters remoteTaskQueueControllerBaseParameters)
            : base(remoteTaskQueueControllerBaseParameters.LoggedInControllerBaseParameters)
        {
            taskViewModelBuilder = remoteTaskQueueControllerBaseParameters.TaskViewModelBuilder;
            objectValueExtracter = remoteTaskQueueControllerBaseParameters.ObjectValueExtracter;
            remoteTaskQueue = remoteTaskQueueControllerBaseParameters.RemoteTaskQueue;
            remoteTaskQueueModelBuilder = remoteTaskQueueControllerBaseParameters.RemoteTaskQueueModelBuilder;
            businessObjectsStorage = remoteTaskQueueControllerBaseParameters.BusinessObjectsStorage;
            criterionBuilder = remoteTaskQueueControllerBaseParameters.MonitoringSearchRequestCriterionBuilder;

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Run(int? pageNumber, string searchRequestId)
        {
            var model = remoteTaskQueueModelBuilder.Build(PageModelBaseParameters, pageNumber, searchRequestId);
            return View("RemoteTaskQueueListView", model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Search(RemoteTaskQueuePageModel pageModel)
        {
            var requestId = Guid.NewGuid().ToString();
            var searchRequest = new MonitoringSearchRequest
                {
                    Id = requestId,
                    ScopeId = requestId,
                    Name = pageModel.SearchPanel.TaskName,
                    TaskId = pageModel.SearchPanel.TaskId,
                    ParentTaskId = pageModel.SearchPanel.ParentTaskId,
                    States = pageModel.SearchPanel.States.Where(x => x.Value == true).Select(x => x.Key).ToArray(),
                    Ticks = new DateTimeRange
                        {
                            From = DateAndTime.ToDateTime(pageModel.SearchPanel.Ticks.From),
                            To = DateAndTime.ToDateTime(pageModel.SearchPanel.Ticks.To)
                        },
                    StartExecutingTicks = new DateTimeRange
                        {
                            From = DateAndTime.ToDateTime(pageModel.SearchPanel.StartExecutedTicks.From),
                            To = DateAndTime.ToDateTime(pageModel.SearchPanel.StartExecutedTicks.To)
                        },
                    MinimalStartTicks = new DateTimeRange
                        {
                            From = DateAndTime.ToDateTime(pageModel.SearchPanel.MinimalStartTicks.From),
                            To = DateAndTime.ToDateTime(pageModel.SearchPanel.MinimalStartTicks.To)
                        }
                };
            businessObjectsStorage.Write(searchRequest);
            return Json(new SuccessOperationResult
                {
                    RedirectTo = Url.Action("Run", new {searchRequestId = requestId})
                });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Show(string id, int? pageNumber, string searchRequestId)
        {
            var remoteTaskInfo = remoteTaskQueue.GetTaskInfo(id);
            return View("RemoteTaskQueueDetailsView", taskViewModelBuilder.Build(remoteTaskInfo, pageNumber, searchRequestId));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Cancel(string id)
        {
            if(!remoteTaskQueue.CancelTask(id))
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
            if(!remoteTaskQueue.RerunTask(id, TimeSpan.FromTicks(0)))
                return ReturnOperationResult(new UnsuccessOperationResult {ErrorMessage = "Задача не может перезапущена"});
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
            var taskData = remoteTaskQueue.GetTaskInfo(id).TaskData;
            var value = objectValueExtracter.Extract(taskData.GetType(), taskData, path);
            if(value.GetType() != typeof(byte[]))
                throw new Exception(string.Format("Type of property by path '{0}' has type '{1}' instead of '{2}'", path, value.GetType(), typeof(byte[])));
            var fileDownloadName = string.Format("{0}_{1}.xml", DateTime.UtcNow.ToString("yyyy.MM.dd hh:mm:ss"), Guid.NewGuid());
            return File((byte[])value, "application/xml", fileDownloadName);
        }

        private readonly ITaskViewModelBuilder taskViewModelBuilder;
        private readonly IObjectValueExtracter objectValueExtracter;
        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly IRemoteTaskQueueModelBuilder remoteTaskQueueModelBuilder;
        private readonly IBusinessObjectsStorage businessObjectsStorage;
        private readonly IMonitoringSearchRequestCriterionBuilder criterionBuilder;
    }
}