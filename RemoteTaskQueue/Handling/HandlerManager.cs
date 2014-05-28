using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.LocalTasks.TaskQueue;

using log4net;

namespace RemoteQueue.Handling
{
    public class HandlerManager : IHandlerManager
    {
        public HandlerManager(
            ITaskQueue taskQueue,
            ITaskCounter taskCounter,
            IShardingManager shardingManager,
            Func<Tuple<string, ColumnInfo>, long, HandlerTask> createHandlerTask,
            TaskHandlerCollection taskHandlerCollection,
            IHandleTasksMetaStorage handleTasksMetaStorage)
        {
            this.taskQueue = taskQueue;
            this.taskCounter = taskCounter;
            this.shardingManager = shardingManager;
            this.createHandlerTask = createHandlerTask;
            this.taskHandlerCollection = taskHandlerCollection;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
        }

        public void Run()
        {
            lock(lockObject)
            {
                var nowTicks = DateTime.UtcNow.Ticks;
                var taskInfos = handleTasksMetaStorage.GetAllTasksInStates(nowTicks, TaskState.New, TaskState.WaitingForRerun, TaskState.InProcess, TaskState.WaitingForRerunAfterError);
                if(logger.IsDebugEnabled)
                    logger.DebugFormat("Начали обработку очереди.");
                foreach(var taskInfo in taskInfos)
                {
                    if(!taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue)) return;
                    QueueTask(taskInfo, null, nowTicks, TaskQueueReason.PullFromQueue);
                }
            }
        }

        public string Id { get { return "HandlerManager"; } }

        public void Start()
        {
            taskQueue.Start();
        }

        public void Stop()
        {
            if(!started)
                return;
            started = false;
            taskQueue.StopAndWait(100 * 1000);
        }

        public long GetQueueLength()
        {
            return taskQueue.GetQueueLength();
        }

        public Tuple<long, long> GetCassandraQueueLength()
        {
            var allTasksInStates = handleTasksMetaStorage.GetAllTasksInStates(DateTime.UtcNow.Ticks, TaskState.New, TaskState.WaitingForRerun, TaskState.InProcess, TaskState.WaitingForRerunAfterError);
            long all = 0;
            long forMe = 0;
            foreach(var allTasksInState in allTasksInStates)
            {
                all++;
                if(shardingManager.IsSituableTask(allTasksInState.Item1))
                    forMe++;
            }
            return new Tuple<long, long>(all, forMe);
        }

        internal void QueueTask(Tuple<string, ColumnInfo> taskInfo, TaskMetaInformation meta, long nowTicks, TaskQueueReason reason)
        {
            if(meta != null && !taskHandlerCollection.ContainsHandlerFor(meta.Name))
                return;
            if(!shardingManager.IsSituableTask(taskInfo.Item1))
                return;
            var handlerTask = createHandlerTask(taskInfo, nowTicks);
            handlerTask.Reason = reason;
            taskQueue.QueueTask(handlerTask);
        }

        private static readonly ILog logger = LogManager.GetLogger(typeof(HandlerManager));

        private readonly Func<Tuple<string, ColumnInfo>, long, HandlerTask> createHandlerTask;
        private readonly TaskHandlerCollection taskHandlerCollection;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly object lockObject = new object();
        private readonly ITaskQueue taskQueue;
        private readonly ITaskCounter taskCounter;
        private readonly IShardingManager shardingManager;
        private volatile bool started;
    }
}