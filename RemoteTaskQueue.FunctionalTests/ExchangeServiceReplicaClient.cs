using System;

using GroBuf;

using JetBrains.Annotations;

using SKBKontur.Catalogue.ServiceLib.GroboClusterClient;

namespace RemoteTaskQueue.FunctionalTests
{
    public class ExchangeServiceReplicaClient : HttpClientForTestsBase
    {
        public ExchangeServiceReplicaClient([NotNull] ISerializer serializer, int port)
            : base(serializer, "ExchangeService", port)
        {
        }

        public void Start()
        {
            clusterClient.Post("Start", new RequestSettings(timeout : TimeSpan.FromSeconds(30)));
        }

        public void Stop()
        {
            clusterClient.Post("Stop", new RequestSettings(timeout : TimeSpan.FromMinutes(2)));
        }

        public void ChangeTaskTtl(TimeSpan ttl)
        {
            clusterClient.Post("ChangeTaskTtl", ttl);
        }
    }
}