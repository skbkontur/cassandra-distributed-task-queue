﻿using System;
using System.Linq;
using System.Web.Mvc;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.AccessControl.AccessRules;
using SKBKontur.Catalogue.CassandraStorageCore.BusinessObjectStorageImpl;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.Core.Web.Models.DateAndTimeModels;
using SKBKontur.Catalogue.Expressions;
using SKBKontur.Catalogue.ObjectManipulation.Extender;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskDetails;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskList;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    [ResourseGroup(ResourseGroups.AdminResourse)]
    public abstract class RemoteTaskQueueControllerBase : AuthanticatedControllerBase
    {
        protected RemoteTaskQueueControllerBase(RemoteTaskQueueControllerBaseParameters remoteTaskQueueControllerBaseParameters)
            : base(remoteTaskQueueControllerBaseParameters.LoggedInControllerBaseParameters)
        {
            extender = remoteTaskQueueControllerBaseParameters.CatalogueExtender;
            taskDetailsModelBuilder = remoteTaskQueueControllerBaseParameters.TaskDetailsModelBuilder;
            objectValueExtracter = remoteTaskQueueControllerBaseParameters.ObjectValueExtracter;
            remoteTaskQueue = remoteTaskQueueControllerBaseParameters.RemoteTaskQueue;
            taskListModelBuilder = remoteTaskQueueControllerBaseParameters.TaskListModelBuilder;
            businessObjectsStorage = remoteTaskQueueControllerBaseParameters.BusinessObjectsStorage;
            monitoringSearchRequestCriterionBuilder = remoteTaskQueueControllerBaseParameters.MonitoringSearchRequestCriterionBuilder;
            remoteTaskQueueMonitoringServiceStorage = remoteTaskQueueControllerBaseParameters.RemoteTaskQueueMonitoringServiceStorage;
            taskListModelHtmlBuilder = remoteTaskQueueControllerBaseParameters.TaskListHtmlModelBuilder;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Run(string searchRequestId, int pageNumber = 0)
        {
            MonitoringSearchRequest searchRequest;
            if(string.IsNullOrEmpty(searchRequestId) || !businessObjectsStorage.TryRead(searchRequestId, searchRequestId, out searchRequest))
                searchRequest = new MonitoringSearchRequest();
            extender.Extend(searchRequest);

            var criterion = monitoringSearchRequestCriterionBuilder.BuildCriterion(searchRequest);

            var rangeFrom = pageNumber * tasksPerPageCount;
            var totalCount = remoteTaskQueueMonitoringServiceStorage.GetCount(criterion);
            var fullTaskMetaInfos = remoteTaskQueueMonitoringServiceStorage.RangeSearch(criterion, rangeFrom, tasksPerPageCount, x => x.MinimalStartTicks.Descending()).ToArray();

            var modelData = taskListModelBuilder.Build(searchRequest, fullTaskMetaInfos, totalCount);

            var pageModel = new TaskListPageModel(PageModelBaseParameters, modelData);
            var totalPagesCount = (totalCount + tasksPerPageCount - 1) / tasksPerPageCount;
            pageModel.PaginatorModelData = new TaskListPaginatorModelData
                {
                    PageNumber = pageNumber,
                    TotalPagesCount = totalPagesCount,
                    PagesWindowSize = 3,
                    SearchRequestId = searchRequestId ?? ""
                };
            pageModel.HtmlModel = taskListModelHtmlBuilder.Build(pageModel);
            return View("TaskList", pageModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Search(TaskListModelData pageModelData)
        {
            var requestId = Guid.NewGuid().ToString();
            var searchRequest = new MonitoringSearchRequest
                {
                    Id = requestId,
                    ScopeId = requestId,
                    Name = pageModelData.SearchPanel.TaskName,
                    TaskId = pageModelData.SearchPanel.TaskId,
                    ParentTaskId = pageModelData.SearchPanel.ParentTaskId,
                    States = (pageModelData.SearchPanel.States ?? new Pair<TaskState, bool?>[0]).Where(x => x.Value == true).Select(x => x.Key).ToArray(),
                    Ticks = new DateTimeRange
                        {
                            From = DateAndTime.ToDateTime(pageModelData.SearchPanel.Ticks.From, DateTimeKind.Unspecified).UtcToMoscowDateTime(),
                            To = DateAndTime.ToDateTime(pageModelData.SearchPanel.Ticks.To, DateTimeKind.Unspecified).UtcToMoscowDateTime()
                        },
                    StartExecutingTicks = new DateTimeRange
                        {
                            From = DateAndTime.ToDateTime(pageModelData.SearchPanel.StartExecutedTicks.From, DateTimeKind.Unspecified).UtcToMoscowDateTime(),
                            To = DateAndTime.ToDateTime(pageModelData.SearchPanel.StartExecutedTicks.To, DateTimeKind.Unspecified).UtcToMoscowDateTime()
                        },
                    FinishExecutingTicks = new DateTimeRange
                        {
                            From = DateAndTime.ToDateTime(pageModelData.SearchPanel.FinishExecutedTicks.From, DateTimeKind.Unspecified).UtcToMoscowDateTime(),
                            To = DateAndTime.ToDateTime(pageModelData.SearchPanel.FinishExecutedTicks.To, DateTimeKind.Unspecified).UtcToMoscowDateTime()
                        },
                    MinimalStartTicks = new DateTimeRange
                        {
                            From = DateAndTime.ToDateTime(pageModelData.SearchPanel.MinimalStartTicks.From, DateTimeKind.Unspecified).UtcToMoscowDateTime(),
                            To = DateAndTime.ToDateTime(pageModelData.SearchPanel.MinimalStartTicks.To, DateTimeKind.Unspecified).UtcToMoscowDateTime()
                        }
                };
            businessObjectsStorage.Write(searchRequest);
            return Json(new SuccessOperationResult
                {
                    NeedRedirect = true,
                    RedirectTo = Url.Action("Run", new {searchRequestId = requestId}),
                });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Show(string id, int pageNumber = 0, string searchRequestId = null)
        {
            var remoteTaskInfo = remoteTaskQueue.GetTaskInfo(id);
            return View("TaskDetails", taskDetailsModelBuilder.Build(remoteTaskInfo, pageNumber, searchRequestId));
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
            var fileDownloadName = string.Format("{0}_{1}.data", DateTime.UtcNow.ToString("yyyy.MM.dd hh:mm:ss"), Guid.NewGuid());
            return File((byte[])value, "application/octet-stream", fileDownloadName);
        }

        private readonly ITaskDetailsModelBuilder taskDetailsModelBuilder;
        private readonly IObjectValueExtracter objectValueExtracter;
        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly ITaskListModelBuilder taskListModelBuilder;
        private readonly IBusinessObjectsStorage businessObjectsStorage;
        private readonly ICatalogueExtender extender;
        private readonly IMonitoringSearchRequestCriterionBuilder monitoringSearchRequestCriterionBuilder;
        private readonly IRemoteTaskQueueMonitoringServiceStorage remoteTaskQueueMonitoringServiceStorage;
        private readonly ITaskListHtmlModelBuilder taskListModelHtmlBuilder;

        private const int tasksPerPageCount = 100;
    }
}