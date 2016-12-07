using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Types;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public interface ITaskIndexController
    {
        void ProcessNewEvents();
        void SetMinTicksHack(long minTicks);
        ElasticMonitoringStatus GetStatus();
        void LogStatus();
        void SendActualizationLagToGraphite();
    }
}