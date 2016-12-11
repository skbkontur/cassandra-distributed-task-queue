using RemoteTaskQueue.Monitoring.Storage.Types;

namespace RemoteTaskQueue.Monitoring.Indexer
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