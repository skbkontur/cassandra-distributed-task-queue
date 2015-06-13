using System;

using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.ClientLib.HttpClientBases;
using SKBKontur.Catalogue.ClientLib.HttpClientBases.Configuration;
using SKBKontur.Catalogue.ClientLib.Topology;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Types;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Client
{
    public class ElasticMonitoringServiceClientImpl : HttpClientBase, IElasticMonitoringServiceClient
    {
        private readonly TaskSearchIndexDataTestService testService;

        public ElasticMonitoringServiceClientImpl(IDomainTopologyFactory domainTopologyFactory, IMethodDomainFactory methodDomainFactory, IHttpServiceClientConfiguration configuration,
            TaskSearchIndexDataTestService testService)
            : base(domainTopologyFactory, methodDomainFactory, configuration)
        {
            this.testService = testService;
        }

        public void UpdateAndFlush()
        {
            Method("Update").SendToEachReplica(DomainConsistencyLevel.All);
            testService.Refresh();
        }

        public void DeleteAll()
        {
            Method("ForgetOldTasks").SendToEachReplica(DomainConsistencyLevel.All);
            testService.DeleteAll();
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
            return "elasticRemoteTaskQueueMonitoring";
        }
    }
}