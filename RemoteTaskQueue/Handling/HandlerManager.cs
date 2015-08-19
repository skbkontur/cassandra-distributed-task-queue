using System;
using System.Linq;

using log4net;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Tracing;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Handling
{
    public class HandlerManager : IHandlerManager
    {
        public HandlerManager(ILocalTaskQueue localTaskQueue, ITaskCounter taskCounter, IHandleTasksMetaStorage handleTasksMetaStorage)
        {
            this.localTaskQueue = localTaskQueue;
            this.taskCounter = taskCounter;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
        }

        public string Id { get { return "HandlerManager"; } }

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
                        var taskId = taskInfo.Item1;
                        if(taskMeta != null && taskMeta.Id != taskId)
                            throw new InvalidProgramStateException(string.Format("taskInfo.TaskId ({0}) != taskMeta.TaskId ({1})", taskId, taskMeta.Id));
                        using(var taskTraceContext = new RemoteTaskHandlingTraceContext(taskMeta))
                        {
                            bool queueIsFull;
                            localTaskQueue.QueueTask(taskId, taskInfo.Item2, taskMeta, TaskQueueReason.PullFromQueue, out queueIsFull, taskTraceContext.TaskIsBeingTraced);
                            if(queueIsFull)
                                return;
                        }
                    }
                }
            }
        }

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
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly object lockObject = new object();
        private readonly ILocalTaskQueue localTaskQueue;
        private readonly ITaskCounter taskCounter;
        private volatile bool started;
    }
}