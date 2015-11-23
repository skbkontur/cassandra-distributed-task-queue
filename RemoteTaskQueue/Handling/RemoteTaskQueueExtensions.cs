using JetBrains.Annotations;

namespace RemoteQueue.Handling
{
    public static class RemoteTaskQueueExtensions
    {
        [NotNull]
        public static string ContinueWith<T>([NotNull] this IRemoteTaskQueue remoteTaskQueue, [NotNull] T taskData) where T : ITaskData
        {
            return remoteTaskQueue.CreateTask(taskData).Queue();
        }
    }
}