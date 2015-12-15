using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

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

        [NotNull]
        public TaskIndexRecord AddTask([NotNull] Task task)
        {
            if(task.Meta.Attempts == 0)
                remoteTaskQueueProfiler.ProcessTaskCreation(task.Meta);

            if(task.Meta.MinimalStartTicks <= DateTime.UtcNow.Ticks + 1)
                remoteTaskQueueProfiler.ProcessTaskEnqueueing(task.Meta);

            string taskDataId;
            if(taskDataStorage.TryWrite(task.Data, out taskDataId))
                task.Meta.TaskDataId = taskDataId;
            return handleTasksMetaStorage.AddMeta(task.Meta);
        }

        [NotNull]
        public Task GetTask([NotNull] string taskId)
        {
            return GetTasks(new[] {taskId}).Single();
        }

        [NotNull]
        public List<Task> GetTasks([NotNull] string[] taskIds)
        {
            var taskMetas = handleTasksMetaStorage.GetMetas(taskIds);
            var taskDatas = taskDataStorage.Read(taskMetas.Values.Select(x => x.TaskDataId).ToArray());
            var tasks = new List<Task>();
            foreach(var taskMeta in taskMetas.Values)
            {
                byte[] taskData;
                if(taskDatas.TryGetValue(taskMeta.TaskDataId, out taskData))
                    tasks.Add(new Task {Meta = taskMeta, Data = taskData});
            }
            return tasks;
        }

        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskDataBlobStorage taskDataStorage;
        private readonly IRemoteTaskQueueProfiler remoteTaskQueueProfiler;
    }
}