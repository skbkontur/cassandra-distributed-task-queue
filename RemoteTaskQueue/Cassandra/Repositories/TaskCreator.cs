using System;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Configuration;
using RemoteQueue.Handling;
using RemoteQueue.Handling.ExecutionContext;

namespace RemoteQueue.Cassandra.Repositories
{
    internal class TaskCreator : ITaskCreator
    {
        public TaskCreator(ITaskDataBlobStorage taskDataStorage, ISerializer serializer, ITaskDataRegistry taskDataRegistry)
        {
            this.taskDataStorage = taskDataStorage;
            this.serializer = serializer;
            this.taskDataRegistry = taskDataRegistry;
        }

        [NotNull]
        public Task Create<T>(T taskData, CreateTaskOptions createTaskOptions)
        {
            createTaskOptions = createTaskOptions ?? new CreateTaskOptions();
            var nowTicks = DateTime.UtcNow.Ticks;
            var type = taskData.GetType();
            var data = serializer.Serialize(type, taskData);
            var taskId = taskDataStorage.GenerateBlobId(data);
            return new Task
                {
                    Data = data,
                    Meta = new TaskMetaInformation(taskDataRegistry.GetTaskName(type), taskId)
                        {
                            Attempts = 0,
                            Ticks = nowTicks,
                            ParentTaskId = string.IsNullOrEmpty(createTaskOptions.ParentTaskId) ? GetCurrentExecutingTaskId() : createTaskOptions.ParentTaskId,
                            TaskGroupLock = createTaskOptions.TaskGroupLock,
                            State = TaskState.New,
                            MinimalStartTicks = 0,
                        }
                };
        }

        private static string GetCurrentExecutingTaskId()
        {
            var context = TaskExecutionContext.Current;
            if(context == null)
                return null;
            return context.CurrentTask.Meta.Id;
        }

        private readonly ITaskDataBlobStorage taskDataStorage;
        private readonly ISerializer serializer;
        private readonly ITaskDataRegistry taskDataRegistry;
    }
}