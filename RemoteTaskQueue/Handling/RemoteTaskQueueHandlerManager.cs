using System;
using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.LocalTasks.TaskQueue;

using log4net;

namespace RemoteQueue.Handling
{
    public class RemoteTaskQueueHandlerManager : IRemoteTaskQueueHandlerManager
    {
        public RemoteTaskQueueHandlerManager(
            ITaskQueue taskQueue,
            ITaskCounter taskCounter,
            IShardingManager shardingManager,
            Func<Tuple<string, ColumnInfo>, HandlerTask> createHandlerTask,
            IHandleTasksMetaStorage handleTasksMetaStorage,
            IRemoteTaskQueue remoteTaskQueue)
        {
            this.taskQueue = taskQueue;
            this.taskCounter = taskCounter;
            this.shardingManager = shardingManager;
            this.createHandlerTask = createHandlerTask;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.remoteTaskQueue = remoteTaskQueue;
        }

        public void Run()
        {
            lock(lockObject)
            {
                IEnumerable<Tuple<string, ColumnInfo>> taskInfos = handleTasksMetaStorage.GetAllTasksInStates(DateTime.UtcNow.Ticks, TaskState.New, TaskState.WaitingForRerun, TaskState.InProcess, TaskState.WaitingForRerunAfterError);
                if(logger.IsDebugEnabled)
                    logger.DebugFormat("Начали обработку очереди.");
                foreach(var taskInfo in taskInfos)
                {
                    if(!shardingManager.IsSituableTask(taskInfo.Item1)) continue;
                    if(!taskCounter.CanQueueTask()) return;
                    taskQueue.QueueTask(createHandlerTask(taskInfo));
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
            taskQueue.StopAndWait();
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

        public void CancelAllTasks()
        {
            var allTasksInStates = handleTasksMetaStorage.GetAllTasksInStates(DateTime.UtcNow.Ticks, TaskState.New, TaskState.WaitingForRerun, TaskState.InProcess, TaskState.WaitingForRerunAfterError);
            foreach(var task in allTasksInStates)
            {
                remoteTaskQueue.CancelTask(task.Item1);
            }
        }

        private static readonly ILog logger = LogManager.GetLogger(typeof(RemoteTaskQueueHandlerManager));

        private readonly Func<Tuple<string, ColumnInfo>, HandlerTask> createHandlerTask;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly object lockObject = new object();
        private readonly ITaskQueue taskQueue;
        private readonly ITaskCounter taskCounter;
        private readonly IShardingManager shardingManager;
        private volatile bool started;
    }
}