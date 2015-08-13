namespace RemoteQueue.LocalTasks.TaskQueue
{
    public interface ITask
    {
        string Id { get; }
        LocalTaskProcessingResult RunTask();
    }
}