using System;
using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
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
            Func<Tuple<string, ColumnInfo>, HandlerTask> createHandlerTask,
            IHandleTasksMetaStorage handleTasksMetaStorage,
            IGlobalTime globalTime)
        {
            this.taskQueue = taskQueue;
            this.taskCounter = taskCounter;
            this.shardingManager = shardingManager;
            this.createHandlerTask = createHandlerTask;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.globalTime = globalTime;
        }

        public void Run()
        {
            lock(lockObject)
            {
                IEnumerable<Tuple<string, ColumnInfo>> taskInfos = handleTasksMetaStorage.GetAllTasksInStates(globalTime.GetNowTicks(), TaskState.New, TaskState.WaitingForRerun, TaskState.InProcess, TaskState.WaitingForRerunAfterError);
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
            var allTasksInStates = handleTasksMetaStorage.GetAllTasksInStates(globalTime.GetNowTicks(), TaskState.New, TaskState.WaitingForRerun, TaskState.InProcess, TaskState.WaitingForRerunAfterError);
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

        private static readonly ILog logger = LogManager.GetLogger(typeof(HandlerManager));

        private readonly Func<Tuple<string, ColumnInfo>, HandlerTask> createHandlerTask;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly IGlobalTime globalTime;
        private readonly object lockObject = new object();
        private readonly ITaskQueue taskQueue;
        private readonly ITaskCounter taskCounter;
        private readonly IShardingManager shardingManager;
        private volatile bool started;
    }
}