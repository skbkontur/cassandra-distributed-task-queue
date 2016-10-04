using System;
using System.Linq;

using log4net;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.CommonBusinessObjects.ScopedStorage.Extensions;
using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.Core.Web.Models.DateAndTimeModels;
using SKBKontur.Catalogue.Core.Web.Models.ModelConfigurations;
using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.Expressions;
using SKBKontur.Catalogue.ObjectManipulation.Extender;
using SKBKontur.Catalogue.Objects.ValueExtracting;
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
    public class RemoteTaskQueueControllerImpl
    {
        public RemoteTaskQueueControllerImpl(ITaskDetailsModelBuilder taskDetailsModelBuilder,
                                             ITaskDetailsHtmlModelBuilder taskDetailsHtmlModelBuilder,
                                             IRemoteTaskQueueMonitoringServiceStorage remoteTaskQueueMonitoringServiceStorage,
                                             IBusinessObjectStorage businessObjectsStorage,
                                             ICatalogueExtender catalogueExtender,
                                             IMonitoringSearchRequestCriterionBuilder monitoringSearchRequestCriterionBuilder,
                                             IRemoteTaskQueue remoteTaskQueue,
                                             ITaskListModelBuilder taskListModelBuilder,
                                             ITaskListHtmlModelBuilder taskListHtmlModelBuilder,
                                             IWebMutatorsTreeCollection<TaskListModelData> webMutatorsTreeCollection)
        {
            this.catalogueExtender = catalogueExtender;
            this.taskDetailsModelBuilder = taskDetailsModelBuilder;
            this.taskDetailsHtmlModelBuilder = taskDetailsHtmlModelBuilder;
            this.remoteTaskQueue = remoteTaskQueue;
            this.taskListModelBuilder = taskListModelBuilder;
            this.businessObjectsStorage = businessObjectsStorage;
            this.monitoringSearchRequestCriterionBuilder = monitoringSearchRequestCriterionBuilder;
            this.remoteTaskQueueMonitoringServiceStorage = remoteTaskQueueMonitoringServiceStorage;
            this.taskListHtmlModelBuilder = taskListHtmlModelBuilder;
            this.webMutatorsTreeCollection = webMutatorsTreeCollection;
        }

        public TaskListPageModel GetTaskListPageModel(PageModelBaseParameters pageModelBaseParameters, PageModelContext pageModelContext, string searchRequestId, int pageNumber)
        {
            MonitoringSearchRequest searchRequest;
            if(string.IsNullOrEmpty(searchRequestId) || !businessObjectsStorage.InScope<MonitoringSearchRequest>(searchRequestId).TryRead(searchRequestId, out searchRequest))
                searchRequest = new MonitoringSearchRequest();
            catalogueExtender.Extend(searchRequest);

            var criterion = monitoringSearchRequestCriterionBuilder.BuildCriterion(searchRequest);

            var rangeFrom = pageNumber * tasksPerPageCount;
            int totalCount;
            MonitoringTaskMetadata[] fullTaskMetaInfos;
            try
            {
                totalCount = remoteTaskQueueMonitoringServiceStorage.GetCount(criterion);
                fullTaskMetaInfos = remoteTaskQueueMonitoringServiceStorage.RangeSearch(criterion, rangeFrom, tasksPerPageCount, x => x.MinimalStartTicks.Descending() && x.Id.Descending()).ToArray();
            }
            catch(DomainIsDisabledException e)
            {
                logger.Error("Can not build TaskList", e);
                totalCount = 0;
                fullTaskMetaInfos = new MonitoringTaskMetadata[0];
                // TODO вытащить во вьюшку инфу о том, что случился Exception
            }

            var modelData = taskListModelBuilder.Build(searchRequest, fullTaskMetaInfos, totalCount);

            var pageModel = new TaskListPageModel(pageModelBaseParameters, webMutatorsTreeCollection.GetWebMutatorsTree(pageModelContext), modelData);
            var totalPagesCount = (totalCount + tasksPerPageCount - 1) / tasksPerPageCount;
            pageModel.PaginatorModelData = new TaskListPaginatorModelData
                {
                    PageNumber = pageNumber,
                    TotalPagesCount = totalPagesCount,
                    PagesWindowSize = 3,
                    SearchRequestId = searchRequestId ?? ""
                };
            pageModel.HtmlModel = taskListHtmlModelBuilder.Build(pageModel);
            return pageModel;
        }

        public int GetTotalCount(string searchRequestId)
        {
            MonitoringSearchRequest searchRequest;
            if(string.IsNullOrEmpty(searchRequestId) || !businessObjectsStorage.InScope<MonitoringSearchRequest>(searchRequestId).TryRead(searchRequestId, out searchRequest))
                searchRequest = new MonitoringSearchRequest();
            catalogueExtender.Extend(searchRequest);

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
            return totalCount;
        }

        public string Search(TaskListModelData pageModelData)
        {
            var searchRequestId = Guid.NewGuid().ToString();
            var searchRequest = new MonitoringSearchRequest
                {
                    Id = searchRequestId,
                    ScopeId = searchRequestId,
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
            businessObjectsStorage.InScope<MonitoringSearchRequest>(searchRequestId).Write(searchRequest);
            return searchRequestId;
        }

        public TaskDetailsPageModel GetTaskDetailsPageModel(PageModelBaseParameters pageModelBaseParameters, string id, int pageNumber, string searchRequestId, bool showTaskData)
        {
            var remoteTaskInfo = remoteTaskQueue.GetTaskInfo(id);
            var modelData = taskDetailsModelBuilder.Build(remoteTaskInfo, pageNumber, searchRequestId);
            if(!showTaskData)
                modelData.TaskData = new SimpleTaskData();
            var taskDetailsPageModel = new TaskDetailsPageModel(pageModelBaseParameters, modelData)
                {
                    Title = string.Format("Task: {0}", id),
                    PageNumber = pageNumber,
                    SearchRequestId = searchRequestId,
                };
            taskDetailsPageModel.HtmlModel = taskDetailsHtmlModelBuilder.Build(taskDetailsPageModel);
            return taskDetailsPageModel;
        }

        public OperationResult Cancel(string id)
        {
            var taskManipulationResult = remoteTaskQueue.TryCancelTask(id);
            if(taskManipulationResult != TaskManipulationResult.Success)
                return new UnsuccessOperationResult {ErrorMessage = string.Format("Задача не может быть отменена. TaskManipulationResult: {0}", taskManipulationResult)};
            return new SuccessOperationResult
                {
                    ActionDescription = "Задача успешно отменена",
                    NeedRedirect = true,
                };
        }

        public OperationResult Rerun(string id)
        {
            var taskManipulationResult = remoteTaskQueue.TryRerunTask(id, TimeSpan.FromTicks(0));
            if(taskManipulationResult != TaskManipulationResult.Success)
                return new UnsuccessOperationResult {ErrorMessage = string.Format("Задача не может перезапущена. TaskManipulationResult: {0}", taskManipulationResult)};
            return new SuccessOperationResult
                {
                    ActionDescription = "Задача успешно перезапущена",
                    NeedRedirect = true,
                };
        }

        public byte[] GetBytes(string id, string path, out string fileDownloadName)
        {
            var taskData = remoteTaskQueue.GetTaskInfo(id).TaskData;
            var value = ObjectValueExtractor.Extract(taskData.GetType(), taskData, path);
            if(value.GetType() != typeof(byte[]))
                throw new Exception(string.Format("Type of property by path '{0}' has type '{1}' instead of '{2}'", path, value.GetType(), typeof(byte[])));
            fileDownloadName = string.Format("{0}_{1}.data", DateTime.UtcNow.ToString("yyyy.MM.dd hh:mm:ss"), Guid.NewGuid());
            return (byte[])value;
        }

        private const int tasksPerPageCount = 100;
        private readonly ITaskDetailsModelBuilder taskDetailsModelBuilder;
        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly ITaskListModelBuilder taskListModelBuilder;
        private readonly IBusinessObjectStorage businessObjectsStorage;
        private readonly ICatalogueExtender catalogueExtender;
        private readonly IMonitoringSearchRequestCriterionBuilder monitoringSearchRequestCriterionBuilder;
        private readonly IRemoteTaskQueueMonitoringServiceStorage remoteTaskQueueMonitoringServiceStorage;
        private readonly ITaskListHtmlModelBuilder taskListHtmlModelBuilder;
        private readonly ITaskDetailsHtmlModelBuilder taskDetailsHtmlModelBuilder;
        private readonly IWebMutatorsTreeCollection<TaskListModelData> webMutatorsTreeCollection;
        private static readonly ILog logger = LogManager.GetLogger(typeof(RemoteTaskQueueControllerImpl));
    }
}