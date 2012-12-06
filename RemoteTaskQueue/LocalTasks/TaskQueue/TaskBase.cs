namespace RemoteQueue.LocalTasks.TaskQueue
{
    public abstract class TaskBase : ITask
    {
        protected TaskBase(string id)
        {
            Id = id;
        }

        public abstract TaskResult RunTask();

        public string Id { get; private set; }
    }
}