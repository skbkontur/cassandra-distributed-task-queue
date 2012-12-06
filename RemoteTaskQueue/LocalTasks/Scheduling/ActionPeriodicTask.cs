using System;

namespace RemoteQueue.LocalTasks.Scheduling
{
    public class ActionPeriodicTask : IPeriodicTask
    {
        public ActionPeriodicTask(Action action, string id)
        {
            this.action = action;
            this.id = id;
        }

        public void Run()
        {
            action();
        }

        public string Id { get { return id; } }

        private readonly Action action;
        private readonly string id;
    }
}