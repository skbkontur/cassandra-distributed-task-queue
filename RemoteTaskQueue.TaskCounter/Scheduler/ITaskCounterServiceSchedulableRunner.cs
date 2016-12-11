namespace RemoteTaskQueue.TaskCounter.Scheduler
{
    public interface ITaskCounterServiceSchedulableRunner
    {
        void Start();
        void Stop();
    }
}