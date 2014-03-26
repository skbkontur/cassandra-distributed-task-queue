using System;

using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.ClientLib.HttpClientBase;
using SKBKontur.Catalogue.ClientLib.HttpClientBase.Configuration;
using SKBKontur.Catalogue.ClientLib.Topology;
using SKBKontur.Catalogue.Expressions.ExpressionTrees;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.Queries;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient
{
    public class RemoteTaskQueueMonitoringServiceClient : HttpClientBase, IRemoteTaskQueueMonitoringServiceClient
    {
        public RemoteTaskQueueMonitoringServiceClient(IDomainTopologyFactory domainTopologyFactory, IMethodDomainFactory methodDomainFactory, IHttpServiceClientConfiguration configuration)
            : base(domainTopologyFactory, methodDomainFactory, configuration)
        {
        }

        public void ActualizeDatabaseScheme()
        {
            Method("ActualizeDatabaseScheme").SendToEachReplica(DomainConsistencyLevel.All);
        }

        public void DropLocalStorage()
        {
            Method("DropLocalStorage").SendToEachReplica(DomainConsistencyLevel.All);
        }

        public MonitoringTaskMetadata[] Search(ExpressionTree criterion, ExpressionTree sortRules, int count = 1000, int rangeFrom = 0)
        {
            var searchQuery = new MonitoringSearchQuery
                {
                    Criterion = criterion,
                    SortRules = sortRules,
                    RangeFrom = rangeFrom,
                    Count = count,
                };
            return Method("Search").QueryFromRandomReplica<MonitoringTaskMetadata[], MonitoringSearchQuery>(searchQuery);
        }

        public object[] GetDistinctValues(ExpressionTree criterion, ExpressionTree columnPath)
        {
            var monitoringGetDistinctValuesQuery = new MonitoringGetDistinctValuesQuery
                {
                    Criterion = criterion,
                    ColumnPath = columnPath,
                };
            return Method("GetDistinctValues").QueryFromRandomReplica<object[], MonitoringGetDistinctValuesQuery>(monitoringGetDistinctValuesQuery);
        }

        public int GetCount(ExpressionTree criterion)
        {
            var query = new MonitoringGetCountQuery
                {
                    Criterion = criterion
                };
            return Method("GetCount").QueryFromRandomReplica<int, MonitoringGetCountQuery>(query);
        }

        public MonitoringTaskMetadata[] GetTaskWithAllDescendants(string taskId)
        {
            return Method("GetTaskWithAllDescendants").QueryFromRandomReplica<MonitoringTaskMetadata[], string>(taskId);
        }

        protected override IHttpServiceClientConfiguration GetConfiguration()
        {
            return base.GetConfiguration().WithTimeout(TimeSpan.FromSeconds(30));
        }

        protected override string GetDefaultTopologyFileName()
        {
            return "remoteTaskQueueMonitoringServiceTopology";
        }
    }
}