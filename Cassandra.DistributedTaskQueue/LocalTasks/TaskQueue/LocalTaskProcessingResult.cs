namespace SkbKontur.Cassandra.DistributedTaskQueue.LocalTasks.TaskQueue
{
    public enum LocalTaskProcessingResult
    {
        Success,
        Error,
        Rerun,
        Undefined,
    }
}