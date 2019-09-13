using System;

using GroBuf;

using JetBrains.Annotations;

using SKBKontur.Catalogue.ServiceLib.GroboClusterClient;

namespace RemoteTaskQueue.FunctionalTests
{
    public class ExchangeServiceReplicaClient : HttpClientForTestsBase
    {
        public ExchangeServiceReplicaClient([NotNull] ISerializer serializer, int port)
            : base(serializer, "ExchangeService", port, defaultRequestTimeout : TimeSpan.FromSeconds(30))
        {
        }

        public void Start()
        {
            clusterClient.Post("Start");
        }

        public void Stop()
        {
            clusterClient.Post("Stop");
        }

        public void ChangeTaskTtl(TimeSpan ttl)
        {
            clusterClient.Post("ChangeTaskTtl", ttl);
        }
    }
}