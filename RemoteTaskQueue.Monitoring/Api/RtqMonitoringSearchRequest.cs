using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Ranges;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public class RtqMonitoringSearchRequest
    {
        [NotNull]
        public Range<DateTime> EnqueueDateTimeRange { get; set; }

        [CanBeNull]
        public string QueryString { get; set; }

        [CanBeNull]
        public TaskState[] States { get; set; }

        [CanBeNull, ItemNotNull]
        public string[] Names { get; set; }
    }
}