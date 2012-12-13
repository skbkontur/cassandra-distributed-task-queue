using System;
using System.Linq.Expressions;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient
{
    public interface IMonitoringServiceStorage
    {
        int GetCount(Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> criterion);
        TaskMetaInformation[] RangeSearch(Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> criterion, int rangeFrom, int count, Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> sortRules = null);
        object[] GetDistinctValues(Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> criterion, Expression<Func<TaskMetaInformationBusinessObjectWrap, object>> columnPath);
    }
}