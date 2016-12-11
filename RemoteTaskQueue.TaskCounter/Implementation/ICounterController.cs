namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public interface ICounterController
    {
        void Restart(long? newStartTicks);
        TaskCount GetTotalCount();
        void ProcessNewEvents();
    }
}