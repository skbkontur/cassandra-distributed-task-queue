using System;
using System.Collections;
using System.Linq;

using JetBrains.Annotations;

using log4net;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Configuration;
using RemoteQueue.Handling;
using RemoteQueue.Tracing;

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

        public void TryQueueTask([NotNull] TaskIndexRecord taskIndexRecord, [CanBeNull] TaskMetaInformation taskMeta, TaskQueueReason taskQueueReason, out bool queueIsFull, out bool queueIsStopped, out bool taskIsSentToThreadPool, bool taskIsBeingTraced)
        {
            using(var infrastructureTraceContext = new InfrastructureTaskTraceContext(taskIsBeingTraced))
            {
                DoTryQueueTask(taskIndexRecord, taskMeta, taskQueueReason, out queueIsFull, out queueIsStopped, out taskIsSentToThreadPool, taskIsBeingTraced);
                infrastructureTraceContext.Finish(taskIsSentToThreadPool);
            }
        }

        private void DoTryQueueTask([NotNull] TaskIndexRecord taskIndexRecord, [CanBeNull] TaskMetaInformation taskMeta, TaskQueueReason taskQueueReason, out bool queueIsFull, out bool queueIsStopped, out bool taskIsSentToThreadPool, bool taskIsBeingTraced)
        {
            queueIsFull = false;
            queueIsStopped = false;
            taskIsSentToThreadPool = false;
            if(taskMeta != null && !taskHandlerRegistry.ContainsHandlerFor(taskMeta.Name))
                return;
            if(taskMeta == null && taskIndexRecord.MinimalStartTicks >= (DateTime.UtcNow - TimeSpan.FromMinutes(20)).Ticks)
            {
                logger.InfoFormat("Мета для задачи TaskId = {0} еще не записана, ждем", taskIndexRecord.TaskId);
                return;
            }
            if(!taskCounter.TryIncrement(taskQueueReason))
            {
                queueIsFull = true;
                return;
            }
            try
            {
                var handlerTask = new HandlerTask(taskIndexRecord, taskQueueReason, taskMeta, taskHandlerRegistry, remoteTaskQueueInternals);
                lock(lockObject)
                {
                    if(stopped)
                    {
                        queueIsStopped = true;
                        return;
                    }
                    if(hashtable.ContainsKey(taskIndexRecord.TaskId))
                        return;
                    var taskWrapper = new TaskWrapper(taskIndexRecord.TaskId, taskQueueReason, taskIsBeingTraced, handlerTask, this);
                    var asyncTask = Task.Factory.StartNew(taskWrapper.Run);
                    taskIsSentToThreadPool = true;
                    if(!taskWrapper.Finished)
                        hashtable.Add(taskIndexRecord.TaskId, asyncTask);
                }
            }
            finally
            {
                if(!taskIsSentToThreadPool)
                    taskCounter.Decrement(taskQueueReason);
            }
        }

        public void TaskFinished([NotNull] string taskId, TaskQueueReason taskQueueReason, bool taskIsBeingTraced, LocalTaskProcessingResult result)
        {
            lock(lockObject)
                hashtable.Remove(taskId);
            taskCounter.Decrement(taskQueueReason);
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
    }
}