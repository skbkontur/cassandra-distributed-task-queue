using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public class RtqMonitoringSearchResults
    {
        public int TotalCount { get; set; }

        [NotNull, ItemNotNull]
        public TaskMetaInformation[] TaskMetas { get; set; }
    }
}