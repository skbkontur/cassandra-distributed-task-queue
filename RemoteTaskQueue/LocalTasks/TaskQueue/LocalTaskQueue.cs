using System;
using System.Collections;
using System.Linq;

using JetBrains.Annotations;

using log4net;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Configuration;
using RemoteQueue.Handling;
using RemoteQueue.Profiling;
using RemoteQueue.Tracing;

using SKBKontur.Catalogue.Objects;

using Task = System.Threading.Tasks.Task;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    internal class LocalTaskQueue : ILocalTaskQueue
    {
        public LocalTaskQueue(ITaskCounter taskCounter, ITaskHandlerRegistry taskHandlerRegistry, IRemoteTaskQueueInternals remoteTaskQueueInternals)
        {
            this.taskCounter = taskCounter;
            this.taskHandlerRegistry = taskHandlerRegistry;
            this.remoteTaskQueueInternals = remoteTaskQueueInternals;
            Instance = this;
        }

        // NB! адская статика, чтобы внутри процессов consumer'ов continuation-оптимизация работала неявно
        [CanBeNull]
        public static ILocalTaskQueue Instance { get; private set; }

        public void Start()
        {
            lock(lockObject)
                stopped = false;
        }

        public void StopAndWait(TimeSpan timeout)
        {
            if(stopped)
                return;
            Task[] tasks;
            lock(lockObject)
            {
                if(stopped)
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
            using(var infrastructureTraceContext = new InfrastructureTaskTraceContext(taskIsBeingTraced))
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
            if(taskMeta != null && !taskHandlerRegistry.ContainsHandlerFor(taskMeta.Name))
                return LocalTaskQueueingResult.TaskIsSkippedResult;
            if(taskMeta == null && taskIndexRecord.MinimalStartTicks > (Timestamp.Now - HandlerTask.MaxAllowedIndexInconsistencyDuration).Ticks)
            {
                logger.Debug($"Мета для задачи TaskId = {taskIndexRecord.TaskId} еще не записана, ждем {HandlerTask.MaxAllowedIndexInconsistencyDuration}");
                return LocalTaskQueueingResult.TaskIsSkippedResult;
            }
            if(!taskCounter.TryIncrement(taskQueueReason))
                return LocalTaskQueueingResult.QueueIsFullResult;
            try
            {
                var handlerTask = new HandlerTask(taskIndexRecord, taskQueueReason, taskMeta, taskHandlerRegistry, remoteTaskQueueInternals);
                lock(lockObject)
                {
                    if(stopped)
                        return LocalTaskQueueingResult.QueueIsStoppedResult;
                    if(hashtable.ContainsKey(taskIndexRecord.TaskId))
                        return LocalTaskQueueingResult.TaskIsSkippedResult;
                    var groboTraceKey = taskMeta?.Name ?? "TaskMetaIsNotAvailable";
                    var taskWrapper = new TaskWrapper(taskIndexRecord.TaskId, groboTraceKey, taskQueueReason, taskIsBeingTraced, handlerTask, this);
                    var asyncTask = Task.Factory.StartNew(taskWrapper.Run);
                    taskIsSentToThreadPool = true;
                    metricsContext.Meter("TaskIsSentToThreadPool").Mark();
                    if(!taskWrapper.Finished)
                        hashtable.Add(taskIndexRecord.TaskId, asyncTask);
                }
            }
            finally
            {
                if(!taskIsSentToThreadPool)
                    taskCounter.Decrement(taskQueueReason);
            }
            return LocalTaskQueueingResult.SuccessResult;
        }

        public void TaskFinished([NotNull] string taskId, TaskQueueReason taskQueueReason, bool taskIsBeingTraced, LocalTaskProcessingResult result)
        {
            lock(lockObject)
                hashtable.Remove(taskId);
            taskCounter.Decrement(taskQueueReason);
            metricsContext.Meter("TaskIsFinished").Mark();
            if(taskIsBeingTraced)
            {
                InfrastructureTaskTraceContext.Finish();
                RemoteTaskHandlingTraceContext.Finish(result, remoteTaskQueueInternals.GlobalTime.GetNowTicks());
            }
        }

        private readonly ITaskCounter taskCounter;
        private readonly ITaskHandlerRegistry taskHandlerRegistry;
        private readonly IRemoteTaskQueueInternals remoteTaskQueueInternals;
        private readonly Hashtable hashtable = new Hashtable();
        private readonly object lockObject = new object();
        private volatile bool stopped;
        private readonly ILog logger = LogManager.GetLogger(typeof(LocalTaskQueue));
        private readonly MetricsContext metricsContext = MetricsContext.For(nameof(LocalTaskQueue));
    }
}