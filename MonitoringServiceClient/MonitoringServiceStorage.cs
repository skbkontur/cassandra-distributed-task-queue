using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using SKBKontur.Catalogue.Expressions.ExpressionTrees;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient
{
    public class MonitoringServiceStorage : IMonitoringServiceStorage
    {
        public MonitoringServiceStorage(IMonitoringServiceClient monitoringServiceClient, IExpressionTreeConverter converter)
        {
            this.monitoringServiceClient = monitoringServiceClient;
            this.converter = converter;
        }

        public int GetCount(Expression<Func<MonitoringTaskMetadata, bool>> criterion)
        {
            return 0;
        }

        public IEnumerable<MonitoringTaskMetadata> RangeSearch(Expression<Func<MonitoringTaskMetadata, bool>> criterion, int rangeFrom, int count, Expression<Func<MonitoringTaskMetadata, bool>> sortRules = null)
        {
            return monitoringServiceClient.Search(converter.ToExpressionTree(criterion), converter.ToExpressionTree(sortRules), count, rangeFrom);
        }

        public IEnumerable<object> GetDistinctValues(Expression<Func<MonitoringTaskMetadata, bool>> criterion, Expression<Func<MonitoringTaskMetadata, object>> columnPath)
        {
            return monitoringServiceClient.GetDistinctValues(converter.ToExpressionTree(criterion), converter.ToExpressionTree(columnPath));
        }

        private readonly IMonitoringServiceClient monitoringServiceClient;
        private readonly IExpressionTreeConverter converter;
    }
}