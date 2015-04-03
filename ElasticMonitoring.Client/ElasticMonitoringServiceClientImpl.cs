using System;

using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.ClientLib.HttpClientBases;
using SKBKontur.Catalogue.ClientLib.HttpClientBases.Configuration;
using SKBKontur.Catalogue.ClientLib.Topology;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Types;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Client
{
    public class ElasticMonitoringServiceClientImpl : HttpClientBase, IElasticMonitoringServiceClient
    {
        public ElasticMonitoringServiceClientImpl(IDomainTopologyFactory domainTopologyFactory, IMethodDomainFactory methodDomainFactory, IHttpServiceClientConfiguration configuration)
            : base(domainTopologyFactory, methodDomainFactory, configuration)
        {
        }

        public void UpdateAndFlush()
        {
            Method("UpdateAndFlush").SendToEachReplica(DomainConsistencyLevel.All);
        }

        public void DeleteAll()
        {
            Method("DeleteAll").SendToEachReplica(DomainConsistencyLevel.All);
        }

        public ElasticMonitoringStatus GetStatus()
        {
            //note метод кривой, работает только с одной репликой
            return Method("GetStatus").InvokeOnRandomReplica().ThanReturn<ElasticMonitoringStatus>();
        }

        protected override IHttpServiceClientConfiguration DoGetConfiguration(IHttpServiceClientConfiguration defaultConfiguration)
        {
            return defaultConfiguration.WithTimeout(TimeSpan.FromMinutes(1));
        }

        protected override string GetDefaultTopologyFileName()
        {
            return "elasticRemoteTaskQueueMonitoringTopology";
        }
    }
}