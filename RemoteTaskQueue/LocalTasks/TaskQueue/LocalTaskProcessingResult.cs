namespace RemoteQueue.LocalTasks.TaskQueue
{
    public enum LocalTaskProcessingResult
    {
        Success,
        Error,
        Rerun,
        Undefined,
    }
}