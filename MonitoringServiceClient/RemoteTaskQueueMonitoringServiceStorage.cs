using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using SKBKontur.Catalogue.Expressions.ExpressionTrees;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient
{
    public class RemoteTaskQueueMonitoringServiceStorage : IRemoteTaskQueueMonitoringServiceStorage
    {
        public RemoteTaskQueueMonitoringServiceStorage(IRemoteTaskQueueMonitoringServiceClient remoteTaskQueueMonitoringServiceClient, IExpressionTreeConverter converter)
        {
            this.remoteTaskQueueMonitoringServiceClient = remoteTaskQueueMonitoringServiceClient;
            this.converter = converter;
        }

        public int GetCount(Expression<Func<MonitoringTaskMetadata, bool>> criterion)
        {
            return remoteTaskQueueMonitoringServiceClient.GetCount(converter.ToExpressionTree(criterion));
        }

        public IEnumerable<MonitoringTaskMetadata> RangeSearch(Expression<Func<MonitoringTaskMetadata, bool>> criterion, int rangeFrom, int count, Expression<Func<MonitoringTaskMetadata, bool>> sortRules = null)
        {
            return remoteTaskQueueMonitoringServiceClient.Search(converter.ToExpressionTree(criterion), converter.ToExpressionTree(sortRules), count, rangeFrom);
        }

        public IEnumerable<object> GetDistinctValues(Expression<Func<MonitoringTaskMetadata, bool>> criterion, Expression<Func<MonitoringTaskMetadata, object>> columnPath)
        {
            return remoteTaskQueueMonitoringServiceClient.GetDistinctValues(converter.ToExpressionTree(criterion), converter.ToExpressionTree(columnPath));
        }

        private readonly IRemoteTaskQueueMonitoringServiceClient remoteTaskQueueMonitoringServiceClient;
        private readonly IExpressionTreeConverter converter;
    }
}