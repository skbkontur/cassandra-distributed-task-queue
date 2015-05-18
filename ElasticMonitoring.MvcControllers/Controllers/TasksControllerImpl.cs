using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.ValueExtracting;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.TaskDetailsTreeView;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Client;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers
{
    public class TasksControllerImpl
    {
        public TasksControllerImpl(ITaskSearchClient taskSearchClient, IRemoteTaskQueue remoteTaskQueue)
        {
            this.taskSearchClient = taskSearchClient;
            this.remoteTaskQueue = remoteTaskQueue;
        }

        public TaskDetailsModel Details(string id, UrlHelper urlHelper, bool currentUserHasAccessToWriteAction, bool currentUserHasAccessToTaskData)
        {
            var taskData = remoteTaskQueue.GetTaskInfo(id);
            return new TaskDetailsModel
                {
                    AllowControlTaskExecution = currentUserHasAccessToWriteAction,
                    TaskName = taskData.Context.Name,
                    TaskId = taskData.Context.Id,
                    State = taskData.Context.State,
                    EnqueueTime = new DateTime(taskData.Context.Ticks, DateTimeKind.Utc),
                    StartExecutedTime = TickToDateTime(taskData.Context.StartExecutingTicks),
                    FinishExecutedTime = TickToDateTime(taskData.Context.FinishExecutingTicks),
                    MinimalStartTime = TickToDateTime(taskData.Context.MinimalStartTicks),
                    ParentTaskId = taskData.Context.ParentTaskId,
                    ChildTaskIds = remoteTaskQueue.GetChildrenTaskIds(taskData.Context.Id),
                    ExceptionInfo = taskData.With(x => x.ExceptionInfo).Return(x => x.ExceptionMessageInfo, ""),
                    AttemptCount = taskData.Context.Attempts,
                    DetailsTree = BuildDetailsTree(taskData, id, urlHelper, currentUserHasAccessToTaskData)
                };
        }

        public byte[] GetBytes(string id, string path, out string fileDownloadName)
        {
            var taskData = remoteTaskQueue.GetTaskInfo(id).TaskData;
            var value = ObjectValueExtractor.Extract(taskData.GetType(), taskData, path);
            if(value.GetType() != typeof(byte[]))
                throw new Exception(string.Format("Type of property by path '{0}' has type '{1}' instead of '{2}'", path, value.GetType(), typeof(byte[])));
            fileDownloadName = string.Format("{0}_{1}_{2}.data", DateTime.UtcNow.ToString("yyyy.MM.dd hh:mm:ss"), path, id);
            return (byte[])value;
        }

        private static DateTime? TickToDateTime(long? startExecutingTicks)
        {
            if(!startExecutingTicks.HasValue)
                return null;
            return new DateTime(startExecutingTicks.Value, DateTimeKind.Utc);
        }

        public void Cancel(string id)
        {
            remoteTaskQueue.CancelTask(id);
        }

        public void Rerun(string id)
        {
            remoteTaskQueue.RerunTask(id, TimeSpan.FromTicks(0));
        }

        private static ObjectTreeModel BuildDetailsTree(RemoteTaskInfo taskData, string taskId, UrlHelper urlHelper, bool currentUserHasAccessToTaskData)
        {
            if(!currentUserHasAccessToTaskData)
                return null;
            var builder = new ObjectTreeViewBuilder(new TaskDataBuildersProvider());
            var result = builder.Build(taskData.TaskData, new TaskDataBuildingContext {TaskId = taskId, UrlHelper = urlHelper});
            if(result != null)
                result.Name = "TaskData";
            return result;
        }

        public TaskSearchResultsModel BuildResultsByIteratorContext(string iteratorContext, bool currentUserHasAccessToWriteAction, bool currentUserHasAccessToTaskData)
        {
            var taskSearchResponse = taskSearchClient.SearchNext(iteratorContext);

            var tasksIds = taskSearchResponse.Ids;
            var nextScrollId = taskSearchResponse.NextScrollId;
            return new TaskSearchResultsModel
                {
                    AllowControlTaskExecution = currentUserHasAccessToWriteAction,
                    AllowViewTaskData = currentUserHasAccessToTaskData,
                    Tasks = remoteTaskQueue.GetTaskInfos(tasksIds).Select(x => new TaskModel
                        {
                            Id = x.Context.Id,
                            Name = x.Context.Name,
                            State = x.Context.State,
                            EnqueueTime = FromTicks(x.Context.Ticks),
                            StartExecutionTime = FromTicks(x.Context.StartExecutingTicks),
                            FinishExecutionTime = FromTicks(x.Context.FinishExecutingTicks),
                            MinimalStartTime = FromTicks(x.Context.MinimalStartTicks),
                            AttemptCount = x.Context.Attempts,
                            ParentTaskId = x.Context.ParentTaskId,
                        }).ToArray(),
                    IteratorContext = nextScrollId,
                };
        }

        public TaskSearchResultsModel BuildResultsBySearchConditions(TaskSearchConditionsModel taskSearchConditions, bool currentUserHasAccessToWriteAction, bool currentUserHasAccessToTaskData)
        {
            if(taskSearchConditions.RangeStart == null)
                throw new Exception("Range start should be specified");
            var start = taskSearchConditions.RangeStart.Value.Ticks;
            var end = (taskSearchConditions.RangeEnd ?? DateTime.UtcNow).Ticks;
            var taskSearchResponse = taskSearchClient.SearchFirst(new TaskSearchRequest
                {
                    FromTicksUtc = start,
                    ToTicksUtc = end,
                    QueryString = taskSearchConditions.SearchString,
                    TaskNames = taskSearchConditions.TaskNames,
                    TaskStates = taskSearchConditions.TaskStates
                });

            var tasksIds = taskSearchResponse.Ids;
            var total = taskSearchResponse.TotalCount;
            var nextScrollId = taskSearchResponse.NextScrollId;

            var taskSearchResultsModel = new TaskSearchResultsModel
                {
                    AllowControlTaskExecution = currentUserHasAccessToWriteAction,
                    AllowViewTaskData = currentUserHasAccessToTaskData,
                    Tasks = remoteTaskQueue.GetTaskInfos(tasksIds).Select(x => new TaskModel
                        {
                            Id = x.Context.Id,
                            Name = x.Context.Name,
                            State = x.Context.State,
                            EnqueueTime = FromTicks(x.Context.Ticks),
                            StartExecutionTime = FromTicks(x.Context.StartExecutingTicks),
                            FinishExecutionTime = FromTicks(x.Context.FinishExecutingTicks),
                            MinimalStartTime = FromTicks(x.Context.MinimalStartTicks),
                            AttemptCount = x.Context.Attempts,
                            ParentTaskId = x.Context.ParentTaskId,
                        }).ToArray(),
                    IteratorContext = nextScrollId,
                    TotalResultCount = total
                };
            return taskSearchResultsModel;
        }

        public DateTime? ParseDateTime(string start)
        {
            if(string.IsNullOrWhiteSpace(start))
                return null;
            DateTime result;
            if(!DateTime.TryParse(start, CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.AssumeUniversal, out result))
                return null;
            return result;
        }

        private static DateTime? FromTicks(long? ticks)
        {
            if(ticks.HasValue)
                return new DateTime(ticks.Value, DateTimeKind.Utc);
            return null;
        }

        private readonly ITaskSearchClient taskSearchClient;
        private readonly IRemoteTaskQueue remoteTaskQueue;
    }
}