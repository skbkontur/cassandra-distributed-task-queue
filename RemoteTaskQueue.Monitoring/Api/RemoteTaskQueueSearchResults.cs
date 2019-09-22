using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteTaskQueue.Monitoring.Api
{
    public class RemoteTaskQueueSearchResults
    {
        public int TotalCount { get; set; }

        [NotNull, ItemNotNull]
        public TaskMetaInformation[] TaskMetas { get; set; }
    }
}