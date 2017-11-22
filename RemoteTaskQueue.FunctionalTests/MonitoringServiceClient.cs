using System;

using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.ClientLib.HttpClientBases;
using SKBKontur.Catalogue.ClientLib.HttpClientBases.Configuration;
using SKBKontur.Catalogue.ClientLib.Topology;

namespace RemoteTaskQueue.FunctionalTests
{
    public class MonitoringServiceClient : HttpClientBase
    {
        public MonitoringServiceClient(IDomainTopologyFactory domainTopologyFactory, IMethodDomainFactory methodDomainFactory, IHttpServiceClientConfiguration configuration)
            : base(domainTopologyFactory, methodDomainFactory, configuration)
        {
        }

        public void ExecuteForcedFeeding()
        {
            Method("ExecuteForcedFeeding").SendToEachReplica(DomainConsistencyLevel.All);
        }

        public void ResetState()
        {
            Method("ResetState").SendToEachReplica(DomainConsistencyLevel.All);
        }

        public void Stop()
        {
            Method("Stop").SendToEachReplica(DomainConsistencyLevel.All);
        }

        protected override sealed string GetDefaultTopologyFileName()
        {
            return "monitoringService";
        }

        protected override IHttpServiceClientConfiguration DoGetConfiguration(IHttpServiceClientConfiguration defaultConfiguration)
        {
            return defaultConfiguration.WithTimeout(TimeSpan.FromMinutes(1));
        }
    }
}