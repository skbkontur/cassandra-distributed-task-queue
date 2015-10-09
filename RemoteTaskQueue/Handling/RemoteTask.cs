using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Tracing;

namespace RemoteQueue.Handling
{
    internal class RemoteTask : IRemoteTask
    {
        public RemoteTask([NotNull] Task task, IHandleTaskCollection handleTaskCollection)
        {
            this.task = task;
            this.handleTaskCollection = handleTaskCollection;
        }

        [NotNull]
        public string Id { get { return task.Meta.Id; } }

        [NotNull]
        public string Queue()
        {
            return Queue(TimeSpan.FromTicks(0));
        }

        [NotNull]
        public virtual string Queue(TimeSpan delay)
        {
            using(new RemoteTaskInitialTraceContext(task.Meta))
            {
                Publish(delay);
                return Id;
            }
        }

        [NotNull]
        protected TaskIndexRecord Publish(TimeSpan delay)
        {
            using(new PublishTaskTraceContext())
            {
                var delayTicks = Math.Max(delay.Ticks, 0);
                task.Meta.MinimalStartTicks = DateTime.UtcNow.Ticks + delayTicks;
                return handleTaskCollection.AddTask(task);
            }
        }

        protected readonly Task task;
        private readonly IHandleTaskCollection handleTaskCollection;
    }
}