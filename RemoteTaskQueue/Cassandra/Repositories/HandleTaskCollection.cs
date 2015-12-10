using System;
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
            return GetTasks(new[] {taskId}).First();
        }

        [NotNull]
        public Task[] GetTasks([NotNull] string[] taskIds)
        {
            var metasMap = handleTasksMetaStorage.GetMetas(taskIds);
            var taskDatasMap = taskDataStorage.Read(metasMap.Values.Select(x => x.TaskDataId).ToArray());

            return metasMap.Select(pair => new Task {Meta = pair.Value, Data = taskDatasMap.ContainsKey(pair.Value.TaskDataId) ? taskDatasMap[pair.Value.TaskDataId] : null})
                           .Where(task => task.Data != null)
                           .ToArray();
        }

        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskDataBlobStorage taskDataStorage;
        private readonly IRemoteTaskQueueProfiler remoteTaskQueueProfiler;
    }
}