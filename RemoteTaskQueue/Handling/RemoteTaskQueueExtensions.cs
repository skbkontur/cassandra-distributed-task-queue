using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [PublicAPI]
    public static class RemoteTaskQueueExtensions
    {
        [NotNull]
        public static string ContinueWith<T>([NotNull] this IRtqTaskProducer taskProducer, [NotNull] T taskData)
            where T : IRtqTaskData
        {
            return taskProducer.CreateTask(taskData).Queue();
        }
    }
}