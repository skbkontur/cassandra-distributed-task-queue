using System;
using System.Collections.Generic;
using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Configuration;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.InternalApi.Core.Exceptions;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Client;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Api
{
    public class RemoteTaskQueueApiImplControllerImpl : IRemoteTaskQueueApiImpl
    {
        public RemoteTaskQueueApiImplControllerImpl(
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

            var result = FindTasks(searchRequest);
            return new RemoteTaskQueueSearchResults
                {
                    TotalCount = result.TotalCount,
                    TaskMetas = remoteTaskQueue.GetTaskInfos(result.Ids)
                                               .Where(x => x != null)
                                               .Select(x => x.Context)
                                               .Select(TaskMetaToApiModel)
                                               .ToArray(),
                };
        }

        private TaskSearchResponse FindTasks(RemoteTaskQueueSearchRequest searchRequest)
        {
            var result = taskSearchClient.Search(new TaskSearchRequest
                {
                    TaskStates = searchRequest.With(x => x.States).Return(z => z.Select(x => x.ToString()).ToArray(), null),
                    TaskNames = searchRequest.Return(x => x.Names, null),
                    QueryString = searchRequest.With(x => x.QueryString).Unless(string.IsNullOrWhiteSpace).Return(x => x, "*"),
                    FromTicksUtc = searchRequest.EnqueueDateTimeRange.LowerBound.Ticks,
                    ToTicksUtc = searchRequest.EnqueueDateTimeRange.UpperBound.Ticks,
                }, searchRequest.From, searchRequest.Size);
            return result;
        }

        private TaskMetaInformationModel TaskMetaToApiModel(TaskMetaInformation taskMeta)
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
                    State = taskMeta.State,
                    Attempts = taskMeta.Attempts,
                    ParentTaskId = taskMeta.ParentTaskId,
                    TaskGroupLock = taskMeta.TaskGroupLock,
                    TraceId = taskMeta.TraceId,
                    TraceIsActive = taskMeta.TraceIsActive,
                };
        }

        private DateTime TicksToDateTime(long ticks)
        {
            return new DateTime(ticks, DateTimeKind.Utc);
        }

        private DateTime? TicksToDateTime(long? ticks)
        {
            if(ticks == null)
            {
                return null;
            }
            return new DateTime(ticks.Value, DateTimeKind.Utc);
        }

        public RemoteTaskInfoModel GetTaskDetails(string taskId)
        {
            var result = remoteTaskQueue.GetTaskInfos(new[] {taskId}).FirstOrDefault();
            if(result == null)
            {
                throw new NotFoundException(string.Format("Task with id {0} not found", taskId));
            }
            return ToRemoteTaskInfoModel(result);
        }

        private RemoteTaskInfoModel ToRemoteTaskInfoModel(RemoteTaskInfo result)
        {
            return new RemoteTaskInfoModel
                {
                    ExceptionInfos = result.ExceptionInfos,
                    TaskData = result.TaskData,
                    TaskMeta = TaskMetaToApiModel(result.Context),
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
            foreach(var taskId in FindTasks(searchRequest).Ids)
            {
                var taskManipulationResult = remoteTaskQueue.TryRerunTask(taskId, TimeSpan.Zero);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }
        
        public Dictionary<string, TaskManipulationResult> CancelTasksBySearchQuery(RemoteTaskQueueSearchRequest searchRequest)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach(var taskId in FindTasks(searchRequest).Ids)
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