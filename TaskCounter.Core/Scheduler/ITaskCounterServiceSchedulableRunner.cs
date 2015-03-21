namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Scheduler
{
    public interface ITaskCounterServiceSchedulableRunner
    {
        void Start();
        void Stop();
    }
}