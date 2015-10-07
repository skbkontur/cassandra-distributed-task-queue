using System;
using System.Linq;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Tracing;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Handling
{
    public class HandlerManager : IHandlerManager
    {
        public HandlerManager(ILocalTaskQueue localTaskQueue, IHandleTasksMetaStorage handleTasksMetaStorage, IGlobalTime globalTime)
        {
            this.localTaskQueue = localTaskQueue;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.globalTime = globalTime;
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
                            bool queueIsFull, taskIsSentToThreadPool;
                            localTaskQueue.QueueTask(taskId, taskInfo.Item2, taskMeta, TaskQueueReason.PullFromQueue, out queueIsFull, out taskIsSentToThreadPool, taskTraceContext.TaskIsBeingTraced);
                            taskTraceContext.Finish(taskIsSentToThreadPool, () => globalTime.GetNowTicks());
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
            localTaskQueue.StopAndWait(TimeSpan.FromSeconds(100));
        }

        private readonly ILocalTaskQueue localTaskQueue;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly IGlobalTime globalTime;
        private readonly object lockObject = new object();
        private volatile bool started;
    }
}