using JetBrains.Annotations;

using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeed
{
    public class EventFeedGlobalTimeProvider : IGlobalTimeProvider
    {
        public EventFeedGlobalTimeProvider(IGlobalTime globalTime)
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