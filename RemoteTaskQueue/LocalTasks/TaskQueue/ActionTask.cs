using System;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public class ActionTask : SimpleTask
    {
        public ActionTask(Action action, string id)
            : base(id)
        {
            this.action = action;
        }

        public override void Run()
        {
            action();
        }

        private readonly Action action;
    }
}