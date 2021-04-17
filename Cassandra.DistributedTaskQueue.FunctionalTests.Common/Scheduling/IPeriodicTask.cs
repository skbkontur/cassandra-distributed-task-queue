namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.Scheduling
{
    public interface IPeriodicTask
    {
        void Run();
        string Id { get; }
    }
}