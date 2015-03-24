namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Scheduler
{
    public interface IElasticMonitoringServiceSchedulableRunner
    {
        void Start();
        void Stop();
    }
}