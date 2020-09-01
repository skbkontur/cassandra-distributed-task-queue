using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling.ExecutionContext
{
    internal interface ITaskExecutionContext
    {
        Task CurrentTask { get; }
    }
}