using JetBrains.Annotations;

using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeed
{
    internal class RtqEventFeedGlobalTimeProvider : IGlobalTimeProvider
    {
        public RtqEventFeedGlobalTimeProvider(IGlobalTime globalTime)
        {
            this.globalTime = globalTime;
        }

        [NotNull]
        public Timestamp GetNowTimestamp()
        {
            return globalTime.UpdateNowTimestamp();
        }

        private readonly IGlobalTime globalTime;
    }
}