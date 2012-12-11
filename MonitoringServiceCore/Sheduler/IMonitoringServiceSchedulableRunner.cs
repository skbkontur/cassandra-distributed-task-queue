namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Sheduler
{
    public interface IMonitoringServiceSchedulableRunner
    {
        void Start();
        void Stop();
    }
}