using System;

using SKBKontur.Catalogue.Expressions.ExpressionTrees;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient
{
    public interface IRemoteTaskQueueMonitoringServiceClient
    {
        void ActualizeDatabaseScheme();
        void DropLocalStorage();

        MonitoringTaskMetadata[] Search(ExpressionTree criterion, ExpressionTree sortRules, int count = 1000, int rangeFrom = 0);
        object[] GetDistinctValues(ExpressionTree criterion, ExpressionTree columnPath);
        int GetCount(ExpressionTree criterion);
        MonitoringTaskMetadata[] GetTaskWithAllDescendants(string taskId);
    }
}