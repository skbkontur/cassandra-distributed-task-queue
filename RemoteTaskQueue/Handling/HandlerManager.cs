using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using MoreLinqInlined;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using SkbKontur.Cassandra.DistributedTaskQueue.LocalTasks.TaskQueue;
using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;
using SkbKontur.Cassandra.DistributedTaskQueue.Tracing;
using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.TimeBasedUuid;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    internal class HandlerManager : IHandlerManager
    {
        public HandlerManager([NotNull] string queueKeyspace,
                              [NotNull] string taskTopic,
                              int maxRunningTasksCount,
                              ILocalTaskQueue localTaskQueue,
                              IHandleTasksMetaStorage handleTasksMetaStorage,
                              IGlobalTime globalTime,
                              ILog logger)
        {
            Id = $"HandlerManager_{queueKeyspace}_{taskTopic}";
            this.taskTopic = taskTopic;
            this.maxRunningTasksCount = maxRunningTasksCount;
            this.localTaskQueue = localTaskQueue;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.globalTime = globalTime;
            this.logger = logger.ForContext(nameof(HandlerManager));
            allTaskIndexShardKeysToRead = allTaskStatesToRead.Select(x => new TaskIndexShardKey(taskTopic, x)).ToArray();
        }

        [NotNull]
        public string Id { get; }

        [NotNull, ItemNotNull]
        public LiveRecordTicksMarkerState[] GetCurrentLiveRecordTicksMarkers()
        {
            return allTaskIndexShardKeysToRead.Select(x => handleTasksMetaStorage.TryGetCurrentLiveRecordTicksMarker(x) ?? new LiveRecordTicksMarkerState(x, Timestamp.Now.Ticks)).ToArray();
        }

        public void Run()
        {
            var toTicks = Timestamp.Now.Ticks;
            TaskIndexRecord[] taskIndexRecords;
            using (metricsContext.Timer("GetIndexRecords").NewContext())
                taskIndexRecords = handleTasksMetaStorage.GetIndexRecords(toTicks, allTaskIndexShardKeysToRead);
            logger.Info("Number of live minimalStartTicksIndex records for topic '{RtqTaskTopic}': {RecordsCount}",
                        new {RtqTaskTopic = taskTopic, RecordsCount = taskIndexRecords.Length});
            foreach (var taskIndexRecordsBatch in taskIndexRecords.Batch(maxRunningTasksCount, Enumerable.ToArray))
            {
                var taskIds = taskIndexRecordsBatch.Select(x => x.TaskId).ToArray();
                Dictionary<string, TaskMetaInformation> taskMetas;
                using (metricsContext.Timer("GetMetas").NewContext())
                    taskMetas = handleTasksMetaStorage.GetMetas(taskIds);
                foreach (var taskIndexRecord in taskIndexRecordsBatch)
                {
                    if (taskMetas.TryGetValue(taskIndexRecord.TaskId, out var taskMeta) && taskMeta.Id != taskIndexRecord.TaskId)
                        throw new InvalidOperationException($"taskIndexRecord.TaskId ({taskIndexRecord.TaskId}) != taskMeta.TaskId ({taskMeta.Id})");
                    using (var taskTraceContext = new RemoteTaskHandlingTraceContext(taskMeta))
                    {
                        LocalTaskQueueingResult result;
                        using (metricsContext.Timer("TryQueueTask").NewContext())
                            result = localTaskQueue.TryQueueTask(taskIndexRecord, taskMeta, TaskQueueReason.PullFromQueue, taskTraceContext.TaskIsBeingTraced);
                        taskTraceContext.Finish(result.TaskIsSentToThreadPool, () => globalTime.UpdateNowTimestamp().Ticks);
                        if (result.QueueIsFull)
                        {
                            metricsContext.Meter("QueueIsFull").Mark();
                            return;
                        }
                        if (result.QueueIsStopped)
                        {
                            metricsContext.Meter("QueueIsStopped").Mark();
                            return;
                        }
                    }
                }
            }
        }

        private readonly string taskTopic;
        private readonly int maxRunningTasksCount;
        private readonly ILocalTaskQueue localTaskQueue;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly IGlobalTime globalTime;
        private readonly ILog logger;
        private readonly TaskIndexShardKey[] allTaskIndexShardKeysToRead;
        private static readonly TaskState[] allTaskStatesToRead = {TaskState.New, TaskState.WaitingForRerun, TaskState.WaitingForRerunAfterError, TaskState.InProcess};
        private readonly MetricsContext metricsContext = MetricsContext.For(nameof(HandlerManager));
    }
}