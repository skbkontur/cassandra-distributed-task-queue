using System;
using System.Linq.Expressions;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Expressions.ExpressionTrees;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient
{
    public class MonitoringServiceStorage : IMonitoringServiceStorage
    {
        public MonitoringServiceStorage(IMonitoringServiceClient monitoringServiceClient, IExpressionTreeConverter converter)
        {
            this.monitoringServiceClient = monitoringServiceClient;
            this.converter = converter;
        }

        public int GetCount(Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> criterion)
        {
            return 0;
        }

        public TaskMetaInformation[] RangeSearch(Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> criterion, int rangeFrom, int count, Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> sortRules = null)
        {
            return monitoringServiceClient.Search(converter.ToExpressionTree(criterion), converter.ToExpressionTree(sortRules), count, rangeFrom);
        }

        public object[] GetDistinctValues(Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> criterion, Expression<Func<TaskMetaInformationBusinessObjectWrap, object>> columnPath)
        {
            return monitoringServiceClient.GetDistinctValues(converter.ToExpressionTree(criterion), converter.ToExpressionTree(columnPath));
        }

        private readonly IMonitoringServiceClient monitoringServiceClient;
        private readonly IExpressionTreeConverter converter;
    }
}