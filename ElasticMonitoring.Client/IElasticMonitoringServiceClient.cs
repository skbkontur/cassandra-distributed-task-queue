using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Types;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Client
{
    public interface IElasticMonitoringServiceClient
    {
        void UpdateAndFlush();
        void DeleteAll();
        ElasticMonitoringStatus GetStatus();
    }
}