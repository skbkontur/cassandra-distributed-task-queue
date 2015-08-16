using GroBuf;

using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.UserClasses;

using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;

namespace RemoteQueue.Handling
{
    internal class RemoteTaskQueueWithContinuationOptimization : RemoteTaskQueueImpl, IRemoteTaskQueue
    {
        public RemoteTaskQueueWithContinuationOptimization(
            ISerializer serializer,
            IHandleTasksMetaStorage handleTasksMetaStorage,
            IHandleTaskCollection handleTaskCollection,
            IRemoteLockCreator remoteLockCreator,
            IHandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage,
            TaskDataRegistryBase taskDataRegistry,
            IChildTaskIndex childTaskIndex,
            ILocalTaskQueue localTaskQueue)
            : base(serializer, handleTasksMetaStorage, handleTaskCollection, remoteLockCreator, handleTaskExceptionInfoStorage, taskDataRegistry, childTaskIndex)
        {
            this.localTaskQueue = localTaskQueue;
        }

        public IRemoteTask CreateTask<T>(T taskData, CreateTaskOptions createTaskOptions) where T : ITaskData
        {
            var task = CreateTaskImpl(taskData, createTaskOptions);
            return new RemoteTaskWithContinuationOptimization(task, handleTaskCollection, localTaskQueue);
        }

        private readonly ILocalTaskQueue localTaskQueue;
    }
}