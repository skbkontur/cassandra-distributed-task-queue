using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public class TimestampRange
    {
        [NotNull]
        public Timestamp LowerBound { get; set; }

        [NotNull]
        public Timestamp UpperBound { get; set; }
    }
}