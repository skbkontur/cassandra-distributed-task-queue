namespace RemoteTaskQueue.FunctionalTests.Common.Scheduling
{
    public interface IPeriodicTask
    {
        void Run();
        string Id { get; }
    }
}