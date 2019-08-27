using System;

using GroBuf;

using JetBrains.Annotations;

using SKBKontur.Catalogue.ClientLib.GroboClusterClient;

using Vostok.Logging.Abstractions;

namespace RemoteTaskQueue.FunctionalTests
{
    public class ExchangeServiceReplicaClient : HttpClientForTestsBase
    {
        public ExchangeServiceReplicaClient([NotNull] ISerializer serializer, [NotNull] ILog logger, int port)
            : base(serializer, logger, applicationName : "ExchangeService", port)
        {
        }

        public void ChangeTaskTtl(TimeSpan ttl) =>
            clusterClient.Post("ChangeTaskTtl", ttl);
    }
}