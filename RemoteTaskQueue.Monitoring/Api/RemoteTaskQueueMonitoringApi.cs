using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Configuration;
using RemoteQueue.Handling;

using RemoteTaskQueue.Monitoring.Storage.Client;

using SKBKontur.Catalogue.Core.InternalApi.Core;
using SKBKontur.Catalogue.Core.InternalApi.Core.Exceptions;

namespace RemoteTaskQueue.Monitoring.Api
{
    public class RemoteTaskQueueMonitoringApi : IRemoteTaskQueueMonitoringApi
    {
        public RemoteTaskQueueMonitoringApi(IRemoteTaskQueue remoteTaskQueue, TaskSearchClient taskSearchClient, ITaskDataRegistry taskDataRegistry)
        {
            this.remoteTaskQueue = remoteTaskQueue;
            this.taskSearchClient = taskSearchClient;
            this.taskDataRegistry = taskDataRegistry;
        }

        [NotNull, ItemNotNull]
        public string[] GetAllTaksNames()
        {
            return taskDataRegistry.GetAllTaskNames();
        }

        [NotNull]
        public RemoteTaskQueueSearchResults Search([NotNull] RemoteTaskQueueSearchRequest searchRequest, int from, int size)
        {
            if (searchRequest.EnqueueDateTimeRange == null)
                throw new BadRequestException("enqueueDateTimeRange should be specified");
            if (searchRequest.EnqueueDateTimeRange.OpenType != null)
                throw new BadRequestException("Both enqueueDateTimeRange.lowerBound and enqueueDateTimeRange.uppedBound should be specified");

            var searchResult = FindTasks(searchRequest, from, size);
            var taskMetas = remoteTaskQueue.GetTaskMetas(searchResult.Ids);
            var taskListItems = new List<TaskMetaInformation>();
            foreach (var taskId in searchResult.Ids)
            {
                if (taskMetas.TryGetValue(taskId, out var taskMeta))
                    taskListItems.Add(taskMeta);
            }
            return new RemoteTaskQueueSearchResults
                {
                    TotalCount = searchResult.TotalCount,
                    TaskMetas = taskListItems.ToArray(),
                };
        }

        [NotNull]
        private TaskSearchResponse FindTasks([NotNull] RemoteTaskQueueSearchRequest searchRequest, int from, int size)
        {
            return taskSearchClient.Search(CreateTaskSearchRequest(searchRequest), from, size);
        }

        private IEnumerable<string> FindAllTasks([NotNull] RemoteTaskQueueSearchRequest searchRequest)
        {
            const int batchSize = 100;
            var taskSearchRequest = CreateTaskSearchRequest(searchRequest);
            var currentOffset = 0;
            while (true)
            {
                var result = taskSearchClient.Search(taskSearchRequest, currentOffset, batchSize);
                if (result.Ids.Length == 0)
                    yield break;
                foreach (var id in result.Ids)
                    yield return id;
                currentOffset += batchSize;
            }
        }

        [NotNull]
        private static TaskSearchRequest CreateTaskSearchRequest([NotNull] RemoteTaskQueueSearchRequest searchRequest)
        {
            return new TaskSearchRequest
                {
                    TaskStates = searchRequest.States?.Select(x => x.ToString()).ToArray(),
                    TaskNames = searchRequest.Names,
                    QueryString = string.IsNullOrWhiteSpace(searchRequest.QueryString) ? "*" : searchRequest.QueryString,
                    FromTicksUtc = searchRequest.EnqueueDateTimeRange.LowerBound.Ticks,
                    ToTicksUtc = searchRequest.EnqueueDateTimeRange.UpperBound.Ticks,
                };
        }

//        [NotNull]
//        private static TaskMetaInformation TaskMetaToApiModel([NotNull] TaskMetaInformation taskMeta, [CanBeNull] string[] childrenTaskIds)
//        {
//            return new TaskMetaInformationModel
//                {
//                    Id = taskMeta.Id,
//                    Name = taskMeta.Name,
//                    EnqueueDateTime = TicksToDateTime(taskMeta.Ticks),
//                    MinimalStartDateTime = TicksToDateTime(taskMeta.MinimalStartTicks),
//                    StartExecutingDateTime = TicksToDateTime(taskMeta.StartExecutingTicks),
//                    FinishExecutingDateTime = TicksToDateTime(taskMeta.FinishExecutingTicks),
//                    LastModificationDateTime = TicksToDateTime(taskMeta.LastModificationTicks),
//                    ExpirationTimestamp = TicksToDateTime(taskMeta.ExpirationTimestampTicks),
//                    ExpirationModificationDateTime = TicksToDateTime(taskMeta.ExpirationModificationTicks),
//                    ChildTaskIds = childrenTaskIds,
//                    State = taskMeta.State,
//                    Attempts = taskMeta.Attempts,
//                    ParentTaskId = taskMeta.ParentTaskId,
//                    TaskGroupLock = taskMeta.TaskGroupLock,
//                    TraceId = taskMeta.TraceId,
//                    TraceIsActive = taskMeta.TraceIsActive,
//                };
//        }

        [NotNull]
        public RemoteTaskInfoModel GetTaskDetails([NotNull] string taskId)
        {
            var result = remoteTaskQueue.TryGetTaskInfo(taskId);
            if (result == null)
                throw new NotFoundException($"Task with id {taskId} not found");
            return new RemoteTaskInfoModel
                {
                    ExceptionInfos = result.ExceptionInfos,
                    TaskData = result.TaskData,
                    TaskMeta = new Merged<TaskMetaInformation, TaskMetaInformationChildTasks>(
                        result.Context,
                        new TaskMetaInformationChildTasks
                            {
                                ChildTaskIds = remoteTaskQueue.GetChildrenTaskIds(taskId)
                            }
                    ),
                };
        }

        [NotNull]
        public Dictionary<string, TaskManipulationResult> CancelTasks([NotNull, ItemNotNull] string[] ids)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach (var taskId in ids.Distinct())
            {
                var taskManipulationResult = remoteTaskQueue.TryCancelTask(taskId);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        [NotNull]
        public Dictionary<string, TaskManipulationResult> RerunTasks([NotNull, ItemNotNull] string[] ids)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach (var taskId in ids.Distinct())
            {
                var taskManipulationResult = remoteTaskQueue.TryRerunTask(taskId, TimeSpan.Zero);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        [NotNull]
        public Dictionary<string, TaskManipulationResult> RerunTasksBySearchQuery([NotNull] RemoteTaskQueueSearchRequest searchRequest)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach (var taskId in FindAllTasks(searchRequest).Distinct())
            {
                var taskManipulationResult = remoteTaskQueue.TryRerunTask(taskId, TimeSpan.Zero);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        [NotNull]
        public Dictionary<string, TaskManipulationResult> CancelTasksBySearchQuery([NotNull] RemoteTaskQueueSearchRequest searchRequest)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach (var taskId in FindAllTasks(searchRequest).Distinct())
            {
                var taskManipulationResult = remoteTaskQueue.TryCancelTask(taskId);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        public void ResetTicksHolderInMemoryState()
        {
            (remoteTaskQueue as RemoteQueue.Handling.RemoteTaskQueue).ResetTicksHolderInMemoryState();
        }

        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly TaskSearchClient taskSearchClient;
        private readonly ITaskDataRegistry taskDataRegistry;
    }
}