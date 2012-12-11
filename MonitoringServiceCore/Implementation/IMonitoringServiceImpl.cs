using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public interface IMonitoringServiceImpl
    {
        int GetCount();
        TaskMetaInformation[] GetRange(int start, int count);
        void ActualizeDatabaseScheme();
        void DropLocalStorage();
        bool CancelTask(string taskId);
        RemoteTaskInfo GetTaskInfo(string taskId);
    }
}