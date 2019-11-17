using JetBrains.Annotations;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Core.EventFeeds;

namespace RemoteTaskQueue.Monitoring
{
    public class RtqGlobalTimeProvider : IGlobalTimeProvider
    {
        public RtqGlobalTimeProvider(IGlobalTime globalTime)
        {
            this.globalTime = globalTime;
        }

        [NotNull]
        public Timestamp GetNowTimestamp()
        {
            return new Timestamp(globalTime.UpdateNowTicks());
        }

        private readonly IGlobalTime globalTime;
    }
}