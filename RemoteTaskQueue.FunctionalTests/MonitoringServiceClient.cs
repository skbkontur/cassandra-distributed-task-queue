using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.ClientLib.HttpClientBases.Configuration;
using SKBKontur.Catalogue.ClientLib.Topology;
using SKBKontur.Catalogue.Core.EventFeeds.HttpAccess;

namespace RemoteTaskQueue.FunctionalTests
{
    public class MonitoringServiceClient : EventFeedHttpClientBase
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
    }
}