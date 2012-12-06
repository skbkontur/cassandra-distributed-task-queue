namespace RemoteQueue.LocalTasks.Scheduling
{
    public interface IPeriodicTask
    {
        void Run();
        string Id { get; }
    }
}