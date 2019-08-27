using System;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using MoreLinq;

using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.FunctionalTests
{
    public class ExchangeServiceClient
    {
        public ExchangeServiceClient([NotNull] ISerializer serializer)
        {
            var logger = Log.For(this);
            var ports = new[] {4403, 4404, 4405, 4406, 4407};
            replicaClients = ports.Select(port => new ExchangeServiceReplicaClient(serializer, logger, port)).ToArray();
        }

        public void Start() =>
            replicaClients.ForEach(replica => replica.Start());

        public void Stop() =>
            replicaClients.ForEach(replica => replica.Stop());

        public void ChangeTaskTtl(TimeSpan ttl) =>
            replicaClients.ForEach(replica => replica.ChangeTaskTtl(ttl));

        private readonly ExchangeServiceReplicaClient[] replicaClients;
    }
}