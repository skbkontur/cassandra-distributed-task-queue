using System;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Settings;

using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace RemoteQueue.Cassandra.Repositories.GlobalTicksHolder
{
    [Obsolete("todo (andrew, 25.01.2020): remove after avk/singleGlobalTime3 release")]
    public class RtqGlobalTimeProxy : IGlobalTime
    {
        public RtqGlobalTimeProxy(ICassandraCluster cassandraCluster, ISerializer serializer, IRemoteTaskQueueSettings settings)
        {
            rtqTicksHolder = new TicksHolder(cassandraCluster, serializer, settings);
            var globalTimeCfConnection = cassandraCluster.RetrieveColumnFamilyConnection(keySpaceName : "EdiRtqKeyspace", columnFamilyName : "GlobalMaxTicks");
            globalTimeTicksHolder = new MaxTicksHolder(globalTimeCfConnection);
        }

        [NotNull]
        public Timestamp UpdateNowTimestamp()
        {
            var rtqGlobalTicks = UpdateNowTicksInRtq();
            var globalNowTimestamp = UpdateNowTimestampInGlobalTime(rtqGlobalTicks);
            return globalNowTimestamp;
        }

        private long UpdateNowTicksInRtq()
        {
            var newNowTicks = Math.Max(rtqTicksHolder.GetMaxTicks(rtqGlobalTicksName) + PreciseTimestampGenerator.TicksPerMicrosecond, Timestamp.Now.Ticks);
            rtqTicksHolder.UpdateMaxTicks(rtqGlobalTicksName, newNowTicks);
            return newNowTicks;
        }

        [NotNull]
        private Timestamp UpdateNowTimestampInGlobalTime(long globalTicks)
        {
            var prevGlobalTicks = globalTimeTicksHolder.GetMaxTicks(globalTicksKey) ?? 0;
            var newGlobalTicks = Math.Max(prevGlobalTicks + PreciseTimestampGenerator.TicksPerMicrosecond, globalTicks);
            globalTimeTicksHolder.UpdateMaxTicks(globalTicksKey, newGlobalTicks);
            return new Timestamp(newGlobalTicks);
        }

        public void ResetInMemoryState()
        {
            rtqTicksHolder.ResetInMemoryState();
            globalTimeTicksHolder.ResetInMemoryState();
        }

        private const string rtqGlobalTicksName = "GlobalTicks2";
        private const string globalTicksKey = "global_ticks";
        private readonly TicksHolder rtqTicksHolder;
        private readonly MaxTicksHolder globalTimeTicksHolder;
    }
}