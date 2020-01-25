using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Core.EventFeeds;

namespace RemoteTaskQueue.Monitoring
{
    [Obsolete("todo (andrew, 25.01.2020): remove after avk/singleGlobalTime3 release")]
    public class RtqGlobalTimeProvider : IGlobalTimeProvider
    {
        public RtqGlobalTimeProvider(IGlobalTime globalTime)
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