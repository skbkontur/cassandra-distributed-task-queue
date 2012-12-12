using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.ClientLib.Topology;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common
{
    public class ExchangeServiceClient : IExchangeServiceClient
    {
        public ExchangeServiceClient(IDomainTopologyFactory domainTopologyFactory, IMethodDomainFactory methodDomainFactory)
        {
            domainTopology = domainTopologyFactory.Create("exchangeServiceTopology");
            this.methodDomainFactory = methodDomainFactory;
        }

        public void Start()
        {
            var domain = methodDomainFactory.Create("Start", domainTopology, 30 * 1000, clientName);
            domain.SendToEachReplica(DomainConsistencyLevel.All);
        }

        public void Stop()
        {
            var domain = methodDomainFactory.Create("Stop", domainTopology, 30 * 1000, clientName);
            domain.SendToEachReplica(DomainConsistencyLevel.All);
        }

        private readonly IDomainTopology domainTopology;
        private readonly IMethodDomainFactory methodDomainFactory;
        private const string clientName = "ExchangeService";
    }
}