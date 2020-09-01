using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories
{
    public class HandleTaskCollection : IHandleTaskCollection
    {
        public HandleTaskCollection(IHandleTasksMetaStorage handleTasksMetaStorage,
                                    ITaskDataStorage taskDataStorage,
                                    ITaskExceptionInfoStorage taskExceptionInfoStorage,
                                    IRtqProfiler rtqProfiler)
        {
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.taskDataStorage = taskDataStorage;
            this.taskExceptionInfoStorage = taskExceptionInfoStorage;
            this.rtqProfiler = rtqProfiler;
        }

        [NotNull]
        public TaskIndexRecord AddTask([NotNull] Task task)
        {
            var metricsContextForTaskName = MetricsContext.For(task.Meta);
            if (task.Meta.Attempts == 0)
            {
                rtqProfiler.ProcessTaskCreation(task.Meta);
                metricsContextForTaskName.Meter("TasksQueued").Mark();
            }
            using (metricsContextForTaskName.Timer("CreationTime").NewContext())
            {
                task.Meta.TaskDataId = taskDataStorage.Write(task.Meta, task.Data);
                return handleTasksMetaStorage.AddMeta(task.Meta, oldTaskIndexRecord : null);
            }
        }

        public void ProlongTaskTtl([NotNull] TaskMetaInformation taskMeta, [NotNull] byte[] taskData)
        {
            taskDataStorage.Overwrite(taskMeta, taskData);
            taskExceptionInfoStorage.ProlongExceptionInfosTtl(taskMeta);
            handleTasksMetaStorage.ProlongMetaTtl(taskMeta);
        }

        [NotNull]
        public Task GetTask([NotNull] string taskId)
        {
            var taskMeta = handleTasksMetaStorage.GetMeta(taskId);
            var taskData = taskDataStorage.Read(taskMeta);
            if (taskData == null)
                throw new InvalidOperationException($"TaskData not found for: {taskId}");
            return new Task(taskMeta, taskData);
        }

        [CanBeNull]
        public Task TryGetTask([NotNull] string taskId)
        {
            return GetTasks(new[] {taskId}).SingleOrDefault();
        }

        [NotNull]
        public List<Task> GetTasks([NotNull] string[] taskIds)
        {
            var taskMetas = handleTasksMetaStorage.GetMetas(taskIds);
            var taskDatas = taskDataStorage.Read(taskMetas.Values.ToArray());
            var tasks = new List<Task>();
            foreach (var taskMeta in taskMetas.Values)
            {
                if (taskDatas.TryGetValue(taskMeta.Id, out var taskData))
                    tasks.Add(new Task(taskMeta, taskData));
            }
            return tasks;
        }

        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskDataStorage taskDataStorage;
        private readonly ITaskExceptionInfoStorage taskExceptionInfoStorage;
        private readonly IRtqProfiler rtqProfiler;
    }
}