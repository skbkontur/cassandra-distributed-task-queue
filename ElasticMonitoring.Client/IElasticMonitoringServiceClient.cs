namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Client
{
    public interface IElasticMonitoringServiceClient
    {
        void UpdateAndFlush();
    }
}