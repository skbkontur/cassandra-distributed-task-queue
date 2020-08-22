namespace SkbKontur.Cassandra.DistributedTaskQueue.Scheduling
{
    public interface IPeriodicTask
    {
        void Run();
        string Id { get; }
    }
}