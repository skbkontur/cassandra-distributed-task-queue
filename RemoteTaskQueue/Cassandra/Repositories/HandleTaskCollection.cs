using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Profiling;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Cassandra.Repositories
{
    public class HandleTaskCollection : IHandleTaskCollection
    {
        public HandleTaskCollection(IHandleTasksMetaStorage handleTasksMetaStorage, ITaskDataStorage taskDataStorage, IRemoteTaskQueueProfiler remoteTaskQueueProfiler)
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

            task.Meta.TaskDataId = taskDataStorage.Write(task.Meta.Id, task.Data);
            return handleTasksMetaStorage.AddMeta(task.Meta);
        }

        [NotNull]
        public Task GetTask([NotNull] string taskId)
        {
            var taskMeta = handleTasksMetaStorage.GetMeta(taskId);
            var taskData = taskDataStorage.Read(taskMeta);
            if(taskData == null)
                throw new InvalidProgramStateException(string.Format("TaskData not found for: {0}", taskId));
            return new Task(taskMeta, taskData);
        }

        [NotNull]
        public List<Task> GetTasks([NotNull] string[] taskIds)
        {
            var taskMetas = handleTasksMetaStorage.GetMetas(taskIds);
            var taskDatas = taskDataStorage.Read(taskMetas.Values.ToArray());
            var tasks = new List<Task>();
            foreach(var taskMeta in taskMetas.Values)
            {
                byte[] taskData;
                if(taskDatas.TryGetValue(taskMeta.Id, out taskData))
                    tasks.Add(new Task(taskMeta, taskData));
            }
            return tasks;
        }

        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskDataStorage taskDataStorage;
        private readonly IRemoteTaskQueueProfiler remoteTaskQueueProfiler;
    }
}