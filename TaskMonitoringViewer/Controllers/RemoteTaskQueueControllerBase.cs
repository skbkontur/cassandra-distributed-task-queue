using System;
using System.Linq;
using System.Web.Mvc;

using log4net;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.AccessControl.AccessRules;
using SKBKontur.Catalogue.AccessControl.Services;
using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.CommonBusinessObjects.ScopedStorage.Extensions;
using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.Core.Web.Models.DateAndTimeModels;
using SKBKontur.Catalogue.Core.Web.Models.ModelConfigurations;
using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.Expressions;
using SKBKontur.Catalogue.ObjectManipulation.Extender;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskDetails;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskList;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList.SearchPanel;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    [ResourseGroup(ResourseGroups.SupportResourse)]
    public abstract class RemoteTaskQueueControllerBase : AuthenticatedControllerBase
    {
        protected RemoteTaskQueueControllerBase(RemoteTaskQueueControllerBaseParameters remoteTaskQueueControllerBaseParameters)
            : base(remoteTaskQueueControllerBaseParameters.AuthenticatedControllerBaseParameters)
        {
            accessControlService = remoteTaskQueueControllerBaseParameters.AccessControlService;
            extender = remoteTaskQueueControllerBaseParameters.CatalogueExtender;
            taskDetailsModelBuilder = remoteTaskQueueControllerBaseParameters.TaskDetailsModelBuilder;
            taskDetailsHtmlModelBuilder = remoteTaskQueueControllerBaseParameters.TaskDetailsHtmlModelBuilder;
            remoteTaskQueue = remoteTaskQueueControllerBaseParameters.RemoteTaskQueue;
            taskListModelBuilder = remoteTaskQueueControllerBaseParameters.TaskListModelBuilder;
            businessObjectsStorage = remoteTaskQueueControllerBaseParameters.BusinessObjectsStorage;
            monitoringSearchRequestCriterionBuilder = remoteTaskQueueControllerBaseParameters.MonitoringSearchRequestCriterionBuilder;
            remoteTaskQueueMonitoringServiceStorage = remoteTaskQueueControllerBaseParameters.RemoteTaskQueueMonitoringServiceStorage;
            taskListModelHtmlBuilder = remoteTaskQueueControllerBaseParameters.TaskListHtmlModelBuilder;
            webMutatorsTreeCollection = remoteTaskQueueControllerBaseParameters.WebMutatorsTreeCollection;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Run(string searchRequestId, int pageNumber = 0)
        {
            MonitoringSearchRequest searchRequest;
            if(string.IsNullOrEmpty(searchRequestId) || !businessObjectsStorage.InScope<MonitoringSearchRequest>(searchRequestId).TryRead(searchRequestId, out searchRequest))
                searchRequest = new MonitoringSearchRequest();
            extender.Extend(searchRequest);

            var criterion = monitoringSearchRequestCriterionBuilder.BuildCriterion(searchRequest);

            var rangeFrom = pageNumber * tasksPerPageCount;
            int totalCount;
            MonitoringTaskMetadata[] fullTaskMetaInfos;
            try
            {
                totalCount = remoteTaskQueueMonitoringServiceStorage.GetCount(criterion);
                fullTaskMetaInfos = remoteTaskQueueMonitoringServiceStorage.RangeSearch(criterion, rangeFrom, tasksPerPageCount, x => x.Ticks.Descending()).ToArray();
            }
            catch(DomainIsDisabledException e)
            {
                logger.Error("Can not build TaskList", e);
                totalCount = 0;
                fullTaskMetaInfos = new MonitoringTaskMetadata[0];
                // TODO вытащить во вьюшку инфу о том, что случился Exception
            }

            var modelData = taskListModelBuilder.Build(searchRequest, fullTaskMetaInfos, totalCount);

            var pageModel = new TaskListPageModel(PageModelBaseParameters, webMutatorsTreeCollection.GetWebMutatorsTree(new PageModelContext(LanguageProvider)), modelData);
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

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCount(string searchRequestId)
        {
            MonitoringSearchRequest searchRequest;
            if(string.IsNullOrEmpty(searchRequestId) || !businessObjectsStorage.InScope<MonitoringSearchRequest>(searchRequestId).TryRead(searchRequestId, out searchRequest))
                searchRequest = new MonitoringSearchRequest();
            extender.Extend(searchRequest);

            int totalCount;
            var criterion = monitoringSearchRequestCriterionBuilder.BuildCriterion(searchRequest);
            try
            {
                totalCount = remoteTaskQueueMonitoringServiceStorage.GetCount(criterion);
            }
            catch(DomainIsDisabledException e)
            {
                logger.Error("Can not build TaskList", e);
                totalCount = 0;
            }
            return Json(totalCount, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult FullScreenTaskCount(string id)
        {
            return View("FullScreenTaskCount", (object)id);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Search(TaskListModelData pageModelData)
        {
            var requestId = Guid.NewGuid().ToString();
            var searchRequest = new MonitoringSearchRequest
                {
                    Id = requestId,
                    ScopeId = requestId,
                    TaskNames = (pageModelData.SearchPanel.TaskNames ?? new Pair<string, bool?>[0]).Where(x => x.Value == true).Select(x => x.Key).ToArray(),
                    TaskId = pageModelData.SearchPanel.TaskId,
                    ParentTaskId = pageModelData.SearchPanel.ParentTaskId,
                    TaskStates = (pageModelData.SearchPanel.TaskStates ?? new Pair<TaskState, bool?>[0]).Where(x => x.Value == true).Select(x => x.Key).ToArray(),
                    Ticks = new DateTimeRange
                        {
                            From = DateAndTime.ToDateTime(pageModelData.SearchPanel.Ticks.From),
                            To = DateAndTime.ToDateTime(pageModelData.SearchPanel.Ticks.To)
                        },
                    StartExecutingTicks = new DateTimeRange
                        {
                            From = DateAndTime.ToDateTime(pageModelData.SearchPanel.StartExecutedTicks.From),
                            To = DateAndTime.ToDateTime(pageModelData.SearchPanel.StartExecutedTicks.To)
                        },
                    FinishExecutingTicks = new DateTimeRange
                        {
                            From = DateAndTime.ToDateTime(pageModelData.SearchPanel.FinishExecutedTicks.From),
                            To = DateAndTime.ToDateTime(pageModelData.SearchPanel.FinishExecutedTicks.To)
                        },
                    MinimalStartTicks = new DateTimeRange
                        {
                            From = DateAndTime.ToDateTime(pageModelData.SearchPanel.MinimalStartTicks.From),
                            To = DateAndTime.ToDateTime(pageModelData.SearchPanel.MinimalStartTicks.To)
                        }
                };
            businessObjectsStorage.InScope<MonitoringSearchRequest>(requestId).Write(searchRequest);
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
            var modelData = taskDetailsModelBuilder.Build(remoteTaskInfo, pageNumber, searchRequestId);
            var hasAccessToTaskData = HasAccessToTaskData();
            if(!hasAccessToTaskData)
                modelData.TaskData = new SimpleTaskData();
            var pageModel = new TaskDetailsPageModel(PageModelBaseParameters, modelData)
                {
                    Title = string.Format("Task: {0}", id),
                    TaskListUrl = Url.Action("Run", new {pageNumber, searchRequestId}),
                    PageNumber = pageNumber,
                    SearchRequestId = searchRequestId,
                };
            pageModel.HtmlModel = taskDetailsHtmlModelBuilder.Build(pageModel);
            return View("TaskDetails", pageModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ResourseGroup(ResourseGroups.AdminResourse)]
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
        [ResourseGroup(ResourseGroups.AdminResourse)]
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
            var value = ObjectValueExtractor.Extract(taskData.GetType(), taskData, path);
            if(value.GetType() != typeof(byte[]))
                throw new Exception(string.Format("Type of property by path '{0}' has type '{1}' instead of '{2}'", path, value.GetType(), typeof(byte[])));
            var fileDownloadName = string.Format("{0}_{1}.data", DateTime.UtcNow.ToString("yyyy.MM.dd hh:mm:ss"), Guid.NewGuid());
            return File((byte[])value, "application/octet-stream", fileDownloadName);
        }

        protected virtual bool HasAccessToTaskData()
        {
            return accessControlService.CheckAccess(Session.UserId, new ResourseGroupAccessRule {ResourseGroupName = ResourseGroups.SupervisorResourse});
        }

        private readonly ITaskDetailsModelBuilder taskDetailsModelBuilder;
        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly ITaskListModelBuilder taskListModelBuilder;
        private readonly IBusinessObjectStorage businessObjectsStorage;
        private readonly ICatalogueExtender extender;
        private readonly IMonitoringSearchRequestCriterionBuilder monitoringSearchRequestCriterionBuilder;
        private readonly IRemoteTaskQueueMonitoringServiceStorage remoteTaskQueueMonitoringServiceStorage;
        private readonly ITaskListHtmlModelBuilder taskListModelHtmlBuilder;
        private readonly ITaskDetailsHtmlModelBuilder taskDetailsHtmlModelBuilder;
        private readonly IAccessControlService accessControlService;
        private readonly IWebMutatorsTreeCollection<TaskListModelData> webMutatorsTreeCollection;

        private static readonly ILog logger = LogManager.GetLogger(typeof(RemoteTaskQueueControllerBase));
        private const int tasksPerPageCount = 100;
    }
}