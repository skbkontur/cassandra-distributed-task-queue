using System.Net;

using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue
{
    public class CassandraSettings : ICassandraSettings
    {
        public IPEndPoint[] Endpoints
        {
            get
            {
                return new[]
                    {
                        new IPEndPoint(new IPAddress(new byte[] {127, 0, 0, 1}), 9160)
                    };
            }
        }

        public IPEndPoint EndpointForFierceCommands { get { return Endpoints[0]; } }
        public bool AllowNullTimestamp { get { return true; } }
        public int Attempts { get { return 5; } }
        public int Timeout { get { return 6000; } }
        public int FierceTimeout { get { return 10000; } }
        public ConsistencyLevel ReadConsistencyLevel { get { return ConsistencyLevel.QUORUM; } }
        public ConsistencyLevel WriteConsistencyLevel { get { return ConsistencyLevel.QUORUM; } }
        public string ClusterName { get { return "CoreCluster"; } }
        public string QueueKeyspace { get { return "QueueKeyspace"; } }
    }
}