using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Tracing;

namespace RemoteQueue.Handling
{
    internal class RemoteTask : IRemoteTask
    {
        public RemoteTask(Task task, IHandleTaskCollection handleTaskCollection)
        {
            this.task = task;
            this.handleTaskCollection = handleTaskCollection;
        }

        public string Id { get { return task.Meta.Id; } }

        public string Queue()
        {
            return Queue(TimeSpan.FromTicks(0));
        }

        public virtual string Queue(TimeSpan delay)
        {
            using(new RemoteTaskInitialTraceContext(task.Meta))
            {
                Publish(delay);
                return Id;
            }
        }

        protected ColumnInfo Publish(TimeSpan delay)
        {
            using(new PublishTaskTraceContext())
            {
                var nowTicks = DateTime.UtcNow.Ticks;
                var delayTicks = Math.Max(delay.Ticks, 0);
                task.Meta.MinimalStartTicks = Math.Max(task.Meta.MinimalStartTicks, nowTicks + delayTicks) + 1;
                return handleTaskCollection.AddTask(task);
            }
        }

        protected readonly Task task;
        private readonly IHandleTaskCollection handleTaskCollection;
    }
}