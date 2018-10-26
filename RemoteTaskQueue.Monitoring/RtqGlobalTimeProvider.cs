using JetBrains.Annotations;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Objects;

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