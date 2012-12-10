using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient
{
    public interface IMonitoringServiceClient
    {
        int GetCount();
        TaskMetaInformation[] GetRange(int start, int count);
        void DropLocalStorage();
        void ActualizeDatabaseScheme();
    }
}