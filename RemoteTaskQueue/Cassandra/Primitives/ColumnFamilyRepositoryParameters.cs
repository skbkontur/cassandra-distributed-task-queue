using System;

using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace RemoteQueue.Cassandra.Primitives
{
    public class ColumnFamilyRepositoryParameters : IColumnFamilyRepositoryParameters
    {
        public ColumnFamilyRepositoryParameters(ICassandraCluster cassandraCluster, ICassandraSettings settings)
        {
            CassandraCluster = cassandraCluster;
            Settings = settings;
        }

        public ICassandraCluster CassandraCluster { get; private set; }
        public ICassandraSettings Settings { get; private set; }
        public string LockColumnFamilyName { get { return LockColumnFamily; } }

        [Obsolete("burmistrov: Remove it after remote lock migration is completed")]
        public const string LockColumnFamily = "lock";

        public const string NewLockColumnFamily = "RemoteTaskQueueLock";
    }
}