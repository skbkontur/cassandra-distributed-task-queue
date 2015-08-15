using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

namespace RemoteQueue.Handling
{
    internal class RemoteTask : IRemoteTask
    {
        public RemoteTask(IHandleTaskCollection handleTaskCollection, Task task, IGlobalTime globalTime)
        {
            this.handleTaskCollection = handleTaskCollection;
            this.task = task;
            this.globalTime = globalTime;
        }

        public string Queue()
        {
            return Queue(TimeSpan.FromTicks(0));
        }

        public string Queue(TimeSpan delay)
        {
            var delayTicks = Math.Max(delay.Ticks, 0);
            //task.Meta.MinimalStartTicks = Math.Max(task.Meta.MinimalStartTicks, globalTime.UpdateNowTicks() + delayTicks) + 1;
            task.Meta.MinimalStartTicks = Math.Max(task.Meta.MinimalStartTicks, DateTime.UtcNow.Ticks + delayTicks) + 1;
            handleTaskCollection.AddTask(task);
            return Id;
        }

        public string Id { get { return task.Meta.Id; } }
        private readonly IHandleTaskCollection handleTaskCollection;
        private readonly Task task;
        private readonly IGlobalTime globalTime;
    }
}