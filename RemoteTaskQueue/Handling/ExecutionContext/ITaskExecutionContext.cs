using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Handling.ExecutionContext
{
    internal interface ITaskExecutionContext
    {
        Task CurrentTask { get; }
    }
}