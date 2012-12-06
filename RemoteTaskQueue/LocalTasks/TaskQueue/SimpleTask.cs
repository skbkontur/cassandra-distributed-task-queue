namespace RemoteQueue.LocalTasks.TaskQueue
{
    public abstract class SimpleTask : TaskBase
    {
        protected SimpleTask(string id)
            : base(id)
        {
        }

        public abstract void Run();

        public override TaskResult RunTask()
        {
            Run();
            return TaskResult.Finish;
        }
    }
}