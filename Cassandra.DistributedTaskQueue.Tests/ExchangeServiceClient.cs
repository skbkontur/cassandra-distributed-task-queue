using System;
using System.Linq;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
{
    public class ExchangeServiceClient
    {
        public ExchangeServiceClient()
        {
            var ports = new[] {4403, 4404, 4405, 4406, 4407};
            replicaClients = ports.Select(port => new ExchangeServiceReplicaClient(port)).ToArray();
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