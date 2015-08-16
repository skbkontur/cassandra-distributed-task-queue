using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

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

        public string Queue(TimeSpan delay)
        {
            var delayTicks = Math.Max(delay.Ticks, 0);
            task.Meta.MinimalStartTicks = Math.Max(task.Meta.MinimalStartTicks, DateTime.UtcNow.Ticks + delayTicks) + 1;
            handleTaskCollection.AddTask(task);
            return Id;
        }

        private readonly Task task;
        private readonly IHandleTaskCollection handleTaskCollection;
    }
}