using System;

using GroBuf;

using JetBrains.Annotations;

using SKBKontur.Catalogue.ServiceLib.GroboClusterClient;

namespace RemoteTaskQueue.FunctionalTests
{
    public class ExchangeServiceReplicaClient : HttpClientForTestsBase
    {
        public ExchangeServiceReplicaClient([NotNull] ISerializer serializer, int port)
            : base(serializer, applicationName : "ExchangeService", port)
        {
        }

        public void ChangeTaskTtl(TimeSpan ttl) =>
            clusterClient.Post("ChangeTaskTtl", ttl);
    }
}