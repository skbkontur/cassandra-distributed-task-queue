using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface ITaskCreator
    {
        [NotNull]
        Task Create<T>(T taskData, CreateTaskOptions createTaskOptions);
    }
}