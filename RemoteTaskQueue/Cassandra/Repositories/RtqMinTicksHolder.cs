using JetBrains.Annotations;

using RemoteQueue.Settings;

using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.ThriftClient.Clusters;

namespace RemoteQueue.Cassandra.Repositories
{
    public class RtqMinTicksHolder : IMinTicksHolder
    {
        public RtqMinTicksHolder(ICassandraCluster cassandraCluster, IRemoteTaskQueueSettings taskQueueSettings)
        {
            var minTicksCfConnection = cassandraCluster.RetrieveColumnFamilyConnection(taskQueueSettings.NewQueueKeyspace, ColumnFamilyName);
            minTicksHolder = new MinTicksHolder(minTicksCfConnection);
        }

        public long GetMinTicks([NotNull] string name)
        {
            return minTicksHolder.GetMinTicks(name) ?? 0;
        }

        public void UpdateMinTicks([NotNull] string name, long ticks)
        {
            minTicksHolder.UpdateMinTicks(name, ticks);
        }

        public void ResetInMemoryState()
        {
            minTicksHolder.ResetInMemoryState();
        }

        public const string ColumnFamilyName = "GlobalMinTicks";
        private readonly MinTicksHolder minTicksHolder;
    }
}