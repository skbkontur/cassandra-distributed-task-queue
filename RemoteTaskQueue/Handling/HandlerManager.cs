using System;
using System.Collections.Generic;
using System.Linq;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Tracing;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Handling
{
    public class HandlerManager : IHandlerManager
    {
        public HandlerManager(int maxRunningTasksCount, ITaskDataTypeToNameMapper taskDataTypeToNameMapper, ILocalTaskQueue localTaskQueue, IHandleTasksMetaStorage handleTasksMetaStorage, IGlobalTime globalTime)
        {
            this.maxRunningTasksCount = maxRunningTasksCount;
            this.localTaskQueue = localTaskQueue;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.globalTime = globalTime;

            var allTaskStatesToRead = new[] {TaskState.New, TaskState.WaitingForRerun, TaskState.InProcess, TaskState.WaitingForRerunAfterError};
            var allTaskNameAndStatesToReadList = new List<TaskNameAndState>();
            allTaskNameAndStatesToReadList.AddRange(allTaskStatesToRead.Select(x => TaskNameAndState.AnyTaskName(x)));
            foreach(var taskName in taskDataTypeToNameMapper.GetAllTaskNames())
                allTaskNameAndStatesToReadList.AddRange(allTaskStatesToRead.Select(x => new TaskNameAndState(taskName, x)));
            allTaskNameAndStatesToRead = allTaskNameAndStatesToReadList.ToArray();
        }

        public string Id { get { return "HandlerManager"; } }

        public void Run()
        {
            lock(lockObject)
            {
                var nowTicks = DateTime.UtcNow.Ticks;
                var taskIndexRecordsBatches = handleTasksMetaStorage
                    .GetIndexRecords(nowTicks, allTaskNameAndStatesToRead)
                    .Batch(maxRunningTasksCount, Enumerable.ToArray);
                foreach(var taskIndexRecordsBatch in taskIndexRecordsBatches)
                {
                    var taskMetas = handleTasksMetaStorage.GetMetasQuiet(taskIndexRecordsBatch.Select(x => x.TaskId).ToArray());
                    for(var i = 0; i < taskIndexRecordsBatch.Length; i++)
                    {
                        var taskMeta = taskMetas[i];
                        var taskIndexRecord = taskIndexRecordsBatch[i];
                        if(taskMeta != null && taskMeta.Id != taskIndexRecord.TaskId)
                            throw new InvalidProgramStateException(string.Format("taskIndexRecord.TaskId ({0}) != taskMeta.TaskId ({1})", taskIndexRecord.TaskId, taskMeta.Id));
                        using(var taskTraceContext = new RemoteTaskHandlingTraceContext(taskMeta))
                        {
                            bool queueIsFull, taskIsSentToThreadPool;
                            localTaskQueue.QueueTask(taskIndexRecord, taskMeta, TaskQueueReason.PullFromQueue, out queueIsFull, out taskIsSentToThreadPool, taskTraceContext.TaskIsBeingTraced);
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

        private readonly int maxRunningTasksCount;
        private readonly ILocalTaskQueue localTaskQueue;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly IGlobalTime globalTime;
        private readonly object lockObject = new object();
        private volatile bool started;
        private readonly TaskNameAndState[] allTaskNameAndStatesToRead;
    }
}