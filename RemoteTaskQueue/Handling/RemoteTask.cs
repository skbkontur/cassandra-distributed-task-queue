using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

namespace RemoteQueue.Handling
{
    public class RemoteTask : IRemoteTask
    {
        public RemoteTask(IHandleTaskCollection handleTaskCollection, Task task)
        {
            this.handleTaskCollection = handleTaskCollection;
            this.task = task;
        }

        public void Queue()
        {
            Queue(TimeSpan.FromTicks(0));
        }

        public void Queue(TimeSpan delay)
        {
            var delayTicks = Math.Max(delay.Ticks, 0);
            task.Meta.MinimalStartTicks = Math.Max(task.Meta.MinimalStartTicks, DateTime.UtcNow.Ticks + delayTicks) + 1;
            handleTaskCollection.AddTask(task);
        }

        public string Id { get { return task.Meta.Id; } }

        private readonly IHandleTaskCollection handleTaskCollection;
        private readonly Task task;
    }
}