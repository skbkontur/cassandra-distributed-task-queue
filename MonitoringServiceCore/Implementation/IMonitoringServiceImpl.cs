using System;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.Queries;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public interface IMonitoringServiceImpl
    {
        void ActualizeDatabaseScheme();
        void DropLocalStorage();
        void RecalculateInProcess();

        TaskCount GetProcessingTaskCount();
        int GetCount(MonitoringGetCountQuery getCountQuery);
        MonitoringTaskMetadata[] Search(MonitoringSearchQuery searchQuery);
        object[] GetDistinctValues(MonitoringGetDistinctValuesQuery getDistinctValuesQuery);
        MonitoringTaskMetadata[] GetTaskWithAllDescendants(string taskId);
        void RestartProcessgingTaskCounter(DateTime? fromTime);
    }
}