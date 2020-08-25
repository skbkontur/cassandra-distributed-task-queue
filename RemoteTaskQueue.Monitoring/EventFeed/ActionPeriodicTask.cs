using System;

using SkbKontur.Cassandra.DistributedTaskQueue.Scheduling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeed
{
    public class ActionPeriodicTask : IPeriodicTask
    {
        public ActionPeriodicTask(string id, Action action)
        {
            Id = id ?? GetType().Name;
            this.action = action;
        }

        public void Run()
        {
            action();
        }

        public string Id { get; }

        private readonly Action action;
    }
}