using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringService.Implementation
{
    public interface IMonitoringServiceImpl
    {
        int GetCount();
        TaskMetaInformation[] GetRange(int start, int count);
        void ActualizeDatabaseScheme();
        void DropLocalStorage();
    }
}