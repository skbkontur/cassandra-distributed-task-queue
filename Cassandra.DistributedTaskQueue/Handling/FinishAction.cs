namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    public enum FinishAction
    {
        Finish,
        Rerun,
        RerunAfterError,
        Fatal
    }
}