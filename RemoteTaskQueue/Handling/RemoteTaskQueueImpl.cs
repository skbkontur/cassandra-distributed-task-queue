using System;
using System.Linq;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex;
using RemoteQueue.Handling.ExecutionContext;
using RemoteQueue.UserClasses;

using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;

namespace RemoteQueue.Handling
{
    internal class RemoteTaskQueueImpl
    {
        public RemoteTaskQueueImpl(
            ISerializer serializer,
            IHandleTasksMetaStorage handleTasksMetaStorage,
            IHandleTaskCollection handleTaskCollection,
            IRemoteLockCreator remoteLockCreator,
            IHandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage,
            TaskDataRegistryBase taskDataRegistry,
            IChildTaskIndex childTaskIndex)
        {
            this.serializer = serializer;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.handleTaskCollection = handleTaskCollection;
            this.remoteLockCreator = remoteLockCreator;
            this.handleTaskExceptionInfoStorage = handleTaskExceptionInfoStorage;
            this.childTaskIndex = childTaskIndex;
            typeToNameMapper = new TaskDataTypeToNameMapper(taskDataRegistry);
        }

        public bool CancelTask(string taskId)
        {
            IRemoteLock remoteLock;
            if(!remoteLockCreator.TryGetLock(taskId, out remoteLock))
                return false;
            using(remoteLock)
            {
                var meta = handleTasksMetaStorage.GetMeta(taskId);
                if(meta.State == TaskState.New || meta.State == TaskState.WaitingForRerun || meta.State == TaskState.WaitingForRerunAfterError || meta.State == TaskState.InProcess)
                {
                    meta.State = TaskState.Canceled;
                    meta.FinishExecutingTicks = DateTime.UtcNow.Ticks;
                    handleTasksMetaStorage.AddMeta(meta);
                    return true;
                }
                return false;
            }
        }

        public bool RerunTask(string taskId, TimeSpan delay)
        {
            IRemoteLock remoteLock;
            if(!remoteLockCreator.TryGetLock(taskId, out remoteLock))
                return false;
            using(remoteLock)
            {
                var meta = handleTasksMetaStorage.GetMeta(taskId);
                meta.State = TaskState.WaitingForRerun;
                meta.MinimalStartTicks = DateTime.UtcNow.Ticks + delay.Ticks;
                handleTasksMetaStorage.AddMeta(meta);
                return true;
            }
        }

        public RemoteTaskInfo GetTaskInfo(string taskId)
        {
            return GetTaskInfos(new[] {taskId}).First();
        }

        public RemoteTaskInfo<T> GetTaskInfo<T>(string taskId)
            where T : ITaskData
        {
            return GetTaskInfos<T>(new[] {taskId}).First();
        }

        public RemoteTaskInfo[] GetTaskInfos(string[] taskIds)
        {
            var tasks = handleTaskCollection.GetTasks(taskIds);
            var taskExceptionInfos = handleTaskExceptionInfoStorage.ReadExceptionInfosQuiet(taskIds);
            return tasks.Zip(
                taskExceptionInfos,
                (t, e) =>
                new RemoteTaskInfo
                    {
                        Context = t.Meta,
                        TaskData = (ITaskData)serializer.Deserialize(typeToNameMapper.GetTaskType(t.Meta.Name), t.Data),
                        ExceptionInfo = e
                    }).ToArray();
        }

        public RemoteTaskInfo<T>[] GetTaskInfos<T>(string[] taskIds) where T : ITaskData
        {
            return GetTaskInfos(taskIds).Select(ConvertRemoteTaskInfo<T>).ToArray();
        }

        public Task CreateTaskImpl<T>(T taskData, CreateTaskOptions createTaskOptions) where T : ITaskData
        {
            createTaskOptions = createTaskOptions ?? new CreateTaskOptions();
            var nowTicks = DateTime.UtcNow.Ticks;
            var taskId = Guid.NewGuid().ToString();
            var type = taskData.GetType();
            return new Task
                {
                    Data = serializer.Serialize(type, taskData),
                    Meta = new TaskMetaInformation
                        {
                            Attempts = 0,
                            Id = taskId,
                            Ticks = nowTicks,
                            Name = typeToNameMapper.GetTaskName(type),
                            ParentTaskId = string.IsNullOrEmpty(createTaskOptions.ParentTaskId) ? GetCurrentExecutingTaskId() : createTaskOptions.ParentTaskId,
                            TaskGroupLock = createTaskOptions.TaskGroupLock,
                            State = TaskState.New,
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

        public string[] GetChildrenTaskIds(string taskId)
        {
            return childTaskIndex.GetChildTaskIds(taskId);
        }

        private static RemoteTaskInfo<T> ConvertRemoteTaskInfo<T>(RemoteTaskInfo task) where T : ITaskData
        {
            var taskType = task.TaskData.GetType();
            if(!typeof(T).IsAssignableFrom(taskType))
                throw new Exception(string.Format("Type '{0}' is not assignable from '{1}'", typeof(T).FullName, taskType.FullName));
            return new RemoteTaskInfo<T>
                {
                    Context = task.Context,
                    TaskData = (T)task.TaskData,
                    ExceptionInfo = task.ExceptionInfo
                };
        }

        protected readonly IHandleTaskCollection handleTaskCollection;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ISerializer serializer;
        private readonly ITaskDataTypeToNameMapper typeToNameMapper;
        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly IHandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage;
        private readonly IChildTaskIndex childTaskIndex;
    }
}