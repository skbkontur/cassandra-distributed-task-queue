using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IHandleTaskCollection
    {
        [NotNull]
        TaskIndexRecord AddTask([NotNull] Task task);

        void ProlongTask([NotNull] Task task);

        [NotNull]
        Task GetTask([NotNull] string taskId);

        [NotNull]
        List<Task> GetTasks([NotNull] string[] taskIds);
    }
}