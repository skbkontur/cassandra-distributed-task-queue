namespace RemoteQueue.LocalTasks.TaskQueue
{
    public interface ITask
    {
        string Id { get; }
        TaskResult RunTask();
    }
}