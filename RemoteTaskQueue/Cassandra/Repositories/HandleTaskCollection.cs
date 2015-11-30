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

            taskDataStorage.Write(task.Meta.Id, task.Data);
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
            var taskDatasMap = taskDataStorage.Read(taskIds);
            var metasMap = handleTasksMetaStorage.GetMetas(taskIds);

            return taskDatasMap.Select(pair => new Task {Data = pair.Value, Meta = metasMap.ContainsKey(pair.Key) ? metasMap[pair.Key] : null})
                               .Where(task => task.Meta != null)
                               .ToArray();
        }

        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskDataBlobStorage taskDataStorage;
        private readonly IRemoteTaskQueueProfiler remoteTaskQueueProfiler;
    }
}