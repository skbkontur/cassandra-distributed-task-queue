using System;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

namespace RemoteTaskQueue.FunctionalTests
{
    public class ExchangeServiceClient
    {
        public ExchangeServiceClient([NotNull] ISerializer serializer)
        {
            var ports = new[] {4403, 4404, 4405, 4406, 4407};
            replicaClients = ports.Select(port => new ExchangeServiceReplicaClient(serializer, port)).ToArray();
        }

        public void Start()
        {
            foreach (var replica in replicaClients)
                replica.Start();
        }

        public void Stop()
        {
            foreach (var replica in replicaClients)
                replica.Stop();
        }

        public void ChangeTaskTtl(TimeSpan ttl)
        {
            foreach (var replica in replicaClients)
                replica.ChangeTaskTtl(ttl);
        }

        private readonly ExchangeServiceReplicaClient[] replicaClients;
    }
}