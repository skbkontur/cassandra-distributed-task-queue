using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient
{
    public interface IMonitoringServiceClient
    {
        int GetCount();
        TaskMetaInformation[] GetRange(int start, int count);
        void DropLocalStorage();
        void ActualizeDatabaseScheme();
        bool CancelTask(string taskId);
        RemoteTaskInfo GetTaskInfo(string taskId);
    }
}