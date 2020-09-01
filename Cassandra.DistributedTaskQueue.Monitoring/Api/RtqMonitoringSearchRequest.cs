using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public class RtqMonitoringSearchRequest
    {
        [NotNull]
        public TimestampRange EnqueueTimestampRange { get; set; }

        [CanBeNull]
        public string QueryString { get; set; }

        [CanBeNull]
        public TaskState[] States { get; set; }

        [CanBeNull, ItemNotNull]
        public string[] Names { get; set; }
    }
}