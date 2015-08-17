using System;
using System.Linq;

using log4net;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Tracing;

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
                        using(new RemoteTaskHandlingTraceContext(taskMeta))
                            localTaskQueue.QueueTask(taskInfo.Item2, taskMeta, TaskQueueReason.PullFromQueue);
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

        private static readonly ILog logger = LogManager.GetLogger(typeof(HandlerManager));
        private readonly TaskHandlerCollection taskHandlerCollection;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly object lockObject = new object();
        private readonly ILocalTaskQueue localTaskQueue;
        private readonly ITaskCounter taskCounter;
        private volatile bool started;
    }
}