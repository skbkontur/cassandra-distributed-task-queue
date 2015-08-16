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
            ILocalTaskQueue localTaskQueue,
            bool enableContinuationOptimization)
            : base(serializer, handleTasksMetaStorage, handleTaskCollection, remoteLockCreator, handleTaskExceptionInfoStorage, taskDataRegistry, childTaskIndex)
        {
            this.localTaskQueue = localTaskQueue;
            this.enableContinuationOptimization = enableContinuationOptimization;
        }

        public IRemoteTask CreateTask<T>(T taskData, CreateTaskOptions createTaskOptions) where T : ITaskData
        {
            var task = CreateTaskImpl(taskData, createTaskOptions);
            return enableContinuationOptimization
                       ? new RemoteTaskWithContinuationOptimization(task, handleTaskCollection, localTaskQueue)
                       : new RemoteTask(task, handleTaskCollection);
        }

        private readonly ILocalTaskQueue localTaskQueue;
        private readonly bool enableContinuationOptimization;
    }
}