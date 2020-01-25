using JetBrains.Annotations;

using RemoteQueue.Settings;

using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    public class GlobalTimeProxy : IGlobalTime
    {
        public GlobalTimeProxy(ICassandraCluster cassandraCluster, IRemoteTaskQueueSettings taskQueueSettings)
        {
            var maxTicksCfConnection = cassandraCluster.RetrieveColumnFamilyConnection(taskQueueSettings.QueueKeyspace, ColumnFamilyName);
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