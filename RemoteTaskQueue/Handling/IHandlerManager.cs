using RemoteQueue.LocalTasks.Scheduling;

namespace RemoteQueue.Handling
{
    public interface IHandlerManager : IPeriodicTask
    {
        void Start();
        void Stop();
    }
}