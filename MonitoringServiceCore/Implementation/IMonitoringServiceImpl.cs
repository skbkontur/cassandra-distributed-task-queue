using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.Queries;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public interface IMonitoringServiceImpl
    {
        void ActualizeDatabaseScheme();
        void DropLocalStorage();

        int GetCount(MonitoringGetCountQuery getCountQuery);
        TaskMetaInformation[] Search(MonitoringSearchQuery searchQuery);
        object[] GetDistinctValues(MonitoringGetDistinctValuesQuery getDistinctValuesQuery);
        RemoteTaskInfo GetTaskInfo(string taskId);
        bool CancelTask(string taskId);
    }
}