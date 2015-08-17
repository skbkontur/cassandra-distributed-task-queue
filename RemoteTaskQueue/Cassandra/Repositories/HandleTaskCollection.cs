using System;
using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Profiling;

namespace RemoteQueue.Cassandra.Repositories
{
    public class HandleTaskCollection : IHandleTaskCollection
    {
        public HandleTaskCollection(IHandleTasksMetaStorage handleTasksMetaStorage, ITaskDataBlobStorage taskDataStorage, IRemoteTaskQueueProfiler remoteTaskQueueProfiler)
        {
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.taskDataStorage = taskDataStorage;
            this.remoteTaskQueueProfiler = remoteTaskQueueProfiler;
        }

        public ColumnInfo AddTask(Task task)
        {
            if(task.Meta.Attempts == 0)
                remoteTaskQueueProfiler.ProcessTaskCreation(task.Meta);

            if(task.Meta.MinimalStartTicks <= DateTime.UtcNow.Ticks + 1)
                remoteTaskQueueProfiler.ProcessTaskEnqueueing(task.Meta);

            taskDataStorage.Write(task.Meta.Id, task.Data);
            return handleTasksMetaStorage.AddMeta(task.Meta);
        }

        public Task GetTask(string taskId)
        {
            return GetTasks(new[] {taskId}).First();
        }

        public Task[] GetTasks(string[] taskIds)
        {
            var taskDatas = taskDataStorage.ReadQuiet(taskIds);
            var metas = handleTasksMetaStorage.GetMetasQuiet(taskIds);
            return taskDatas.Zip(
                metas,
                (t, m) =>
                new Task
                    {
                        Data = t,
                        Meta = m
                    }
                )
                            .Where(x => x.Meta != null && x.Data != null)
                            .ToArray();
        }

        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskDataBlobStorage taskDataStorage;
        private readonly IRemoteTaskQueueProfiler remoteTaskQueueProfiler;
    }
}