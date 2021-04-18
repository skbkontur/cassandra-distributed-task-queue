using System;
using System.Net;

using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.ThriftClient.Clusters;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common
{
    public class CassandraClusterSettings : ICassandraClusterSettings
    {
        public string ClusterName { get; } = "RtqCluster";
        public ConsistencyLevel ReadConsistencyLevel { get; set; }
        public ConsistencyLevel WriteConsistencyLevel { get; set; }
        public IPEndPoint[] Endpoints { get; set; }
        public IPEndPoint EndpointForFierceCommands { get; set; }
        public bool AllowNullTimestamp => false;
        public int Attempts { get; set; }
        public int Timeout { get; set; }
        public int FierceTimeout { get; set; }
        public TimeSpan? ConnectionIdleTimeout { get; set; }
        public bool EnableMetrics => false;
        public Credentials Credentials => null;
    }
}