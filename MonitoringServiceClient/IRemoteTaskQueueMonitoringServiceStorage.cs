using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient
{
    public interface IRemoteTaskQueueMonitoringServiceStorage
    {
        int GetCount(Expression<Func<MonitoringTaskMetadata, bool>> criterion);
        IEnumerable<MonitoringTaskMetadata> RangeSearch(Expression<Func<MonitoringTaskMetadata, bool>> criterion, int rangeFrom, int count, Expression<Func<MonitoringTaskMetadata, bool>> sortRules = null);
        IEnumerable<object> GetDistinctValues(Expression<Func<MonitoringTaskMetadata, bool>> criterion, Expression<Func<MonitoringTaskMetadata, object>> columnPath);
        IEnumerable<string> GetChildrenTaskIds(string taskId);
    }
}