using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    public class GlobalTimeProxy : IGlobalTime
    {
        public GlobalTimeProxy(ICassandraCluster cassandraCluster, IRtqSettings rtqSettings)
        {
            var maxTicksCfConnection = cassandraCluster.RetrieveColumnFamilyConnection(rtqSettings.QueueKeyspace, ColumnFamilyName);
            globalTime = new GlobalTime(maxTicksCfConnection);
        }

        [NotNull]
        public Timestamp UpdateNowTimestamp()
        {
            return globalTime.UpdateNowTimestamp();
        }

        public void ResetInMemoryState()
        {
            globalTime.ResetInMemoryState();
        }

        public const string ColumnFamilyName = "GlobalMaxTicks";
        private readonly GlobalTime globalTime;
    }
}