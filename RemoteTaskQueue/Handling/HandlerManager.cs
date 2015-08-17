using System;
using System.Linq;

using Kontur.Tracing.Core;

using log4net;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.LocalTasks.TaskQueue;

namespace RemoteQueue.Handling
{
    public class HandlerManager : IHandlerManager
    {
        public HandlerManager(
            ILocalTaskQueue localTaskQueue,
            ITaskCounter taskCounter,
            TaskHandlerCollection taskHandlerCollection,
            IHandleTasksMetaStorage handleTasksMetaStorage)
        {
            this.localTaskQueue = localTaskQueue;
            this.taskCounter = taskCounter;
            this.taskHandlerCollection = taskHandlerCollection;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
        }

        public void Run()
        {
            lock(lockObject)
            {
                var nowTicks = DateTime.UtcNow.Ticks;
                var taskInfoBatches = handleTasksMetaStorage
                    .GetAllTasksInStates(nowTicks, TaskState.New, TaskState.WaitingForRerun, TaskState.InProcess, TaskState.WaitingForRerunAfterError)
                    .Batch(100, Enumerable.ToArray);
                if(logger.IsDebugEnabled)
                    logger.DebugFormat("Начали обработку очереди.");
                foreach(var taskInfoBatch in taskInfoBatches)
                {
                    var taskMetas = handleTasksMetaStorage.GetMetasQuiet(taskInfoBatch.Select(x => x.Item1).ToArray());
                    for(var i = 0; i < taskInfoBatch.Length; i++)
                    {
                        var taskMeta = taskMetas[i];
                        var taskInfo = taskInfoBatch[i];
                        if(taskMeta != null && !taskHandlerCollection.ContainsHandlerFor(taskMeta.Name))
                            return;
                        if(!taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue))
                            return;

                        ITraceContext taskTraceContext = null;
                        if(meta != null)
                        {
                            taskTraceContext = Trace.ContinueContext(meta.TraceId, meta.Id, meta.TraceIsActive, true);
                            taskTraceContext.RecordTimepoint(Timepoint.Start, new DateTime(meta.Ticks, DateTimeKind.Utc));
                        }

                        localTaskQueue.QueueTask(taskInfo.Item2, taskMeta, TaskQueueReason.PullFromQueue);

                        if(taskTraceContext != null)
                            taskTraceContext.Dispose(); // Pop taskTraceContext
                    }
                }
            }
        }

        public string Id { get { return "HandlerManager"; } }

        public void Start()
        {
            localTaskQueue.Start();
        }

        public void Stop()
        {
            if(!started)
                return;
            started = false;
            localTaskQueue.StopAndWait(100 * 1000);
        }

        public long GetQueueLength()
        {
            return localTaskQueue.GetQueueLength();
        }

        public Tuple<long, long> GetCassandraQueueLength()
        {
            var allTasksInStates = handleTasksMetaStorage.GetAllTasksInStates(DateTime.UtcNow.Ticks, TaskState.New, TaskState.WaitingForRerun, TaskState.InProcess, TaskState.WaitingForRerunAfterError);
            long all = allTasksInStates.Count();
            return new Tuple<long, long>(all, all);
            }

        internal void QueueTask(Tuple<string, ColumnInfo> taskInfo, TaskMetaInformation meta, long nowTicks, TaskQueueReason reason)
        {
            if(meta != null && !taskHandlerCollection.ContainsHandlerFor(meta.Name))
                return;
            if(!shardingManager.IsSituableTask(taskInfo.Item1))
                return;
            var handlerTask = createHandlerTask(taskInfo, meta, nowTicks);
            handlerTask.Reason = reason;
            if(meta != null && (reason == TaskQueueReason.PullFromQueue || (reason == TaskQueueReason.TaskContinuation && meta.State == TaskState.New)))
            {
                var infrastructureTraceContext = Trace.CreateChildContext("Handle.Infrastructure");
                infrastructureTraceContext.RecordTimepoint(Timepoint.Start);
                if(taskQueue.QueueTask(handlerTask))
                    infrastructureTraceContext.Dispose(); // Pop infrastructureTraceContext
                else
                {
                    infrastructureTraceContext.RecordTimepoint(Timepoint.Finish);
                    infrastructureTraceContext.Dispose(); // Finish infrastructureTraceContext
                }
            }
        }

        private static readonly ILog logger = LogManager.GetLogger(typeof(HandlerManager));
        private readonly TaskHandlerCollection taskHandlerCollection;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly object lockObject = new object();
        private readonly ILocalTaskQueue localTaskQueue;
        private readonly ITaskCounter taskCounter;
        private volatile bool started;
    }
}