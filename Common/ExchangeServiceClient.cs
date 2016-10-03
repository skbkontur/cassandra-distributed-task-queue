using System;

using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.ClientLib.HttpClientBases;
using SKBKontur.Catalogue.ClientLib.HttpClientBases.Configuration;
using SKBKontur.Catalogue.ClientLib.Topology;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common
{
    public class ExchangeServiceClient : HttpClientBase, IExchangeServiceClient
    {
        public ExchangeServiceClient(IDomainTopologyFactory domainTopologyFactory, IMethodDomainFactory methodDomainFactory, IHttpServiceClientConfiguration configuration)
            : base(domainTopologyFactory, methodDomainFactory, configuration)
        {
        }

        public void Start()
        {
            Method("Start").SendToEachReplica(DomainConsistencyLevel.All);
        }

        public void Stop()
        {
            Method("Stop").SendToEachReplica(DomainConsistencyLevel.All);
        }

        public void ChangeTaskTtl(TimeSpan ttl)
        {
            Method("ChangeTaskTtl").SendToEachReplica(DomainConsistencyLevel.All, ttl);
        }

        protected override IHttpServiceClientConfiguration DoGetConfiguration(IHttpServiceClientConfiguration defaultConfiguration)
        {
            return defaultConfiguration.WithTimeout(TimeSpan.FromSeconds(30));
        }

        protected override string GetDefaultTopologyFileName()
        {
            return "exchangeService";
        }
    }
}