namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    public enum TaskQueueReason
    {
        PullFromQueue,
        TaskContinuation
    }
}