namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringService.Sheduler
{
    public interface IMonitoringServiceSchedulableRunner
    {
        void Start();
        void Stop();
    }
}