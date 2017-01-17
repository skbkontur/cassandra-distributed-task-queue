using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Configuration;
using RemoteQueue.Handling;

using RemoteTaskQueue.Monitoring.Storage.Client;

using SKBKontur.Catalogue.Core.InternalApi.Core.Exceptions;
using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.Api
{
    public class RemoteTaskQueueApiImpl : IRemoteTaskQueueApiImpl
    {
        public RemoteTaskQueueApiImpl(
            IRemoteTaskQueue remoteTaskQueue,
            ITaskSearchClient taskSearchClient,
            ITaskDataRegistry taskDataRegistry
            )
        {
            this.remoteTaskQueue = remoteTaskQueue;
            this.taskSearchClient = taskSearchClient;
            this.taskDataRegistry = taskDataRegistry;
        }

        public string[] GetAllTaksNames()
        {
            return taskDataRegistry.GetAllTaskNames();
        }

        public RemoteTaskQueueSearchResults Search(RemoteTaskQueueSearchRequest searchRequest)
        {
            if(searchRequest.EnqueueDateTimeRange == null)
                throw new BadRequestException("enqueueDateTimeRange should be specified");
            if(searchRequest.EnqueueDateTimeRange.OpenType != null)
                throw new BadRequestException("Both enqueueDateTimeRange.lowerBound and enqueueDateTimeRange.uppedBound should be specified");

            var searchResult = FindTasks(searchRequest);
            var taskMetas = remoteTaskQueue.GetTaskMetas(searchResult.Ids);
            var taskListItems = new List<TaskMetaInformationModel>();
            foreach(var taskId in searchResult.Ids)
            {
                TaskMetaInformation taskMeta;
                if(taskMetas.TryGetValue(taskId, out taskMeta))
                    taskListItems.Add(TaskMetaToApiModel(taskMeta, childrenTaskIds : null));
            }
            return new RemoteTaskQueueSearchResults
                {
                    TotalCount = searchResult.TotalCount,
                    TaskMetas = taskListItems.ToArray(),
                };
        }

        private TaskSearchResponse FindTasks(RemoteTaskQueueSearchRequest searchRequest)
        {
            return taskSearchClient.Search(CreateTaskSearchRequest(searchRequest), searchRequest.From, searchRequest.Size);
        }

        private IEnumerable<string> FindAllTasks(RemoteTaskQueueSearchRequest searchRequest)
        {
            const int batchSize = 100;
            var taskSearchRequest = CreateTaskSearchRequest(searchRequest);
            var currentOffset = 0;
            while(true)
            {
                var result = taskSearchClient.Search(taskSearchRequest, currentOffset, batchSize);
                if(result.Ids.Length == 0)
                    yield break;
                foreach(var id in result.Ids)
                    yield return id;
                currentOffset += batchSize;
            }
        }

        private static TaskSearchRequest CreateTaskSearchRequest(RemoteTaskQueueSearchRequest searchRequest)
        {
            return new TaskSearchRequest
                {
                    TaskStates = searchRequest.With(x => x.States).Return(z => z.Select(x => x.ToString()).ToArray(), null),
                    TaskNames = searchRequest.Return(x => x.Names, null),
                    QueryString = searchRequest.With(x => x.QueryString).Unless(string.IsNullOrWhiteSpace).Return(x => x, "*"),
                    FromTicksUtc = searchRequest.EnqueueDateTimeRange.LowerBound.Ticks,
                    ToTicksUtc = searchRequest.EnqueueDateTimeRange.UpperBound.Ticks,
                };
        }

        [NotNull]
        private static TaskMetaInformationModel TaskMetaToApiModel([NotNull] TaskMetaInformation taskMeta, [CanBeNull] string[] childrenTaskIds)
        {
            return new TaskMetaInformationModel
                {
                    Id = taskMeta.Id,
                    Name = taskMeta.Name,
                    EnqueueDateTime = TicksToDateTime(taskMeta.Ticks),
                    MinimalStartDateTime = TicksToDateTime(taskMeta.MinimalStartTicks),
                    StartExecutingDateTime = TicksToDateTime(taskMeta.StartExecutingTicks),
                    FinishExecutingDateTime = TicksToDateTime(taskMeta.FinishExecutingTicks),
                    LastModificationDateTime = TicksToDateTime(taskMeta.LastModificationTicks),
                    ExpirationTimestamp = TicksToDateTime(taskMeta.ExpirationTimestampTicks),
                    ExpirationModificationDateTime = TicksToDateTime(taskMeta.ExpirationModificationTicks),
                    ChildTaskIds = childrenTaskIds,
                    State = taskMeta.State,
                    Attempts = taskMeta.Attempts,
                    ParentTaskId = taskMeta.ParentTaskId,
                    TaskGroupLock = taskMeta.TaskGroupLock,
                    TraceId = taskMeta.TraceId,
                    TraceIsActive = taskMeta.TraceIsActive,
                };
        }

        private static DateTime TicksToDateTime(long ticks)
        {
            return new DateTime(ticks, DateTimeKind.Utc);
        }

        private static DateTime? TicksToDateTime(long? ticks)
        {
            if(ticks == null)
                return null;
            return new DateTime(ticks.Value, DateTimeKind.Utc);
        }

        public RemoteTaskInfoModel GetTaskDetails(string taskId)
        {
            var result = remoteTaskQueue.TryGetTaskInfo(taskId);
            if(result == null)
                throw new NotFoundException(string.Format("Task with id {0} not found", taskId));
            return new RemoteTaskInfoModel
                {
                    ExceptionInfos = result.ExceptionInfos,
                    TaskData = result.TaskData,
                    TaskMeta = TaskMetaToApiModel(result.Context, remoteTaskQueue.GetChildrenTaskIds(taskId)),
                };
        }

        public Dictionary<string, TaskManipulationResult> CancelTasks(string[] ids)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach(var taskId in ids.Distinct())
            {
                var taskManipulationResult = remoteTaskQueue.TryCancelTask(taskId);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        public Dictionary<string, TaskManipulationResult> RerunTasks(string[] ids)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach(var taskId in ids.Distinct())
            {
                var taskManipulationResult = remoteTaskQueue.TryRerunTask(taskId, TimeSpan.Zero);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        public Dictionary<string, TaskManipulationResult> RerunTasksBySearchQuery(RemoteTaskQueueSearchRequest searchRequest)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach(var taskId in FindAllTasks(searchRequest).Distinct())
            {
                var taskManipulationResult = remoteTaskQueue.TryRerunTask(taskId, TimeSpan.Zero);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        public Dictionary<string, TaskManipulationResult> CancelTasksBySearchQuery(RemoteTaskQueueSearchRequest searchRequest)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach(var taskId in FindAllTasks(searchRequest).Distinct())
            {
                var taskManipulationResult = remoteTaskQueue.TryCancelTask(taskId);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly ITaskSearchClient taskSearchClient;
        private readonly ITaskDataRegistry taskDataRegistry;
    }
}