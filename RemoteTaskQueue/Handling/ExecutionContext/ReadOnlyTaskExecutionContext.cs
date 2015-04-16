using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Handling.ExecutionContext
{
    internal class ReadOnlyTaskExecutionContext : ITaskExecutionContext
    {
        public ReadOnlyTaskExecutionContext(ITaskExecutionContext context)
        {
            this.context = context;
        }

        public Task CurrentTask { get { return context.CurrentTask; } }
        private readonly ITaskExecutionContext context;
    }
}