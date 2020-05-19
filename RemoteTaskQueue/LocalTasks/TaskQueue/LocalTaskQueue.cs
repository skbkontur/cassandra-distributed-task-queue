using System;
using System.Collections;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;
using SkbKontur.Cassandra.DistributedTaskQueue.Tracing;
using SkbKontur.Cassandra.TimeBasedUuid;

using Vostok.Logging.Abstractions;

using Task = System.Threading.Tasks.Task;

namespace SkbKontur.Cassandra.DistributedTaskQueue.LocalTasks.TaskQueue
{
    internal class LocalTaskQueue : ILocalTaskQueue
    {
        public LocalTaskQueue(LocalQueueTaskCounter localQueueTaskCounter, IRtqTaskHandlerRegistry taskHandlerRegistry, IRtqInternals rtqInternals)
        {
            this.localQueueTaskCounter = localQueueTaskCounter;
            this.taskHandlerRegistry = taskHandlerRegistry;
            this.rtqInternals = rtqInternals;
            logger = rtqInternals.Logger.ForContext(nameof(LocalTaskQueue));
            rtqInternals.AttachLocalTaskQueue(this);
        }

        public void Start()
        {
            lock (lockObject)
                stopped = false;
        }

        public void StopAndWait(TimeSpan timeout)
        {
            if (stopped)
                return;
            Task[] tasks;
            lock (lockObject)
            {
                if (stopped)
                    return;
                stopped = true;
                tasks = hashtable.Values.Cast<Task>().ToArray();
                hashtable.Clear();
            }
            Task.WaitAll(tasks, timeout);
        }

        [NotNull]
        public LocalTaskQueueingResult TryQueueTask([NotNull] TaskIndexRecord taskIndexRecord, [CanBeNull] TaskMetaInformation taskMeta, TaskQueueReason taskQueueReason, bool taskIsBeingTraced)
        {
            using (var infrastructureTraceContext = new InfrastructureTaskTraceContext(taskIsBeingTraced))
            {
                var result = DoTryQueueTask(taskIndexRecord, taskMeta, taskQueueReason, taskIsBeingTraced);
                infrastructureTraceContext.Finish(result.TaskIsSentToThreadPool);
                return result;
            }
        }

        [NotNull]
        private LocalTaskQueueingResult DoTryQueueTask([NotNull] TaskIndexRecord taskIndexRecord, [CanBeNull] TaskMetaInformation taskMeta, TaskQueueReason taskQueueReason, bool taskIsBeingTraced)
        {
            var taskIsSentToThreadPool = false;
            if (taskMeta != null && !taskHandlerRegistry.ContainsHandlerFor(taskMeta.Name))
                return LocalTaskQueueingResult.TaskIsSkippedResult;
            if (taskMeta == null && taskIndexRecord.MinimalStartTicks > (Timestamp.Now - HandlerTask.MaxAllowedIndexInconsistencyDuration).Ticks)
            {
                logger.Debug($"Мета для задачи TaskId = {{RtqTaskId}} еще не записана, ждем {HandlerTask.MaxAllowedIndexInconsistencyDuration}", new {RtqTaskId = taskIndexRecord.TaskId});
                return LocalTaskQueueingResult.TaskIsSkippedResult;
            }
            if (!localQueueTaskCounter.TryIncrement(taskQueueReason))
                return LocalTaskQueueingResult.QueueIsFullResult;
            try
            {
                var handlerTask = new HandlerTask(taskIndexRecord, taskQueueReason, taskMeta, taskHandlerRegistry, rtqInternals);
                lock (lockObject)
                {
                    if (stopped)
                        return LocalTaskQueueingResult.QueueIsStoppedResult;
                    if (hashtable.ContainsKey(taskIndexRecord.TaskId))
                        return LocalTaskQueueingResult.TaskIsSkippedResult;
                    var taskWrapper = new TaskWrapper(taskIndexRecord.TaskId, taskQueueReason, taskIsBeingTraced, handlerTask, this, logger);
                    var asyncTask = Task.Factory.StartNew(taskWrapper.Run);
                    taskIsSentToThreadPool = true;
                    metricsContext.Meter("TaskIsSentToThreadPool").Mark();
                    if (!taskWrapper.Finished)
                        hashtable.Add(taskIndexRecord.TaskId, asyncTask);
                }
            }
            finally
            {
                if (!taskIsSentToThreadPool)
                    localQueueTaskCounter.Decrement(taskQueueReason);
            }
            return LocalTaskQueueingResult.SuccessResult;
        }

        public void TaskFinished([NotNull] string taskId, TaskQueueReason taskQueueReason, bool taskIsBeingTraced, LocalTaskProcessingResult result)
        {
            lock (lockObject)
                hashtable.Remove(taskId);
            localQueueTaskCounter.Decrement(taskQueueReason);
            metricsContext.Meter("TaskIsFinished").Mark();
            if (taskIsBeingTraced)
            {
                InfrastructureTaskTraceContext.Finish();
                RemoteTaskHandlingTraceContext.Finish(result, rtqInternals.GlobalTime.UpdateNowTimestamp().Ticks);
            }
        }

        private volatile bool stopped;
        private readonly LocalQueueTaskCounter localQueueTaskCounter;
        private readonly IRtqTaskHandlerRegistry taskHandlerRegistry;
        private readonly IRtqInternals rtqInternals;
        private readonly ILog logger;
        private readonly Hashtable hashtable = new Hashtable();
        private readonly object lockObject = new object();
        private readonly MetricsContext metricsContext = MetricsContext.For(nameof(LocalTaskQueue));
    }
}