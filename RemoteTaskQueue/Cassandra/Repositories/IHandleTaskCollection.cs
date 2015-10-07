using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IHandleTaskCollection
    {
        [NotNull]
        TaskColumnInfo AddTask([NotNull] Task task);

        [NotNull]
        Task GetTask([NotNull] string taskId);

        [NotNull]
        Task[] GetTasks([NotNull] string[] taskIds);
    }
}