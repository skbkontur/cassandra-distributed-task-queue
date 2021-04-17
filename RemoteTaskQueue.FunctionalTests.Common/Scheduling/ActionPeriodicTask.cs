using System;

namespace RemoteTaskQueue.FunctionalTests.Common.Scheduling
{
    public class ActionPeriodicTask : PeriodicTaskBase
    {
        public ActionPeriodicTask(string id, Action action)
            : base(id)
        {
            this.action = action;
        }

        public override sealed void Run()
        {
            action();
        }

        private readonly Action action;
    }
}