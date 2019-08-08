using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.InternalApi.Core;

namespace RemoteTaskQueue.Monitoring.Api
{
    [InternalAPI]
    public class RemoteTaskQueueSearchResults
    {
        public int TotalCount { get; set; }

        [NotNull, ItemNotNull]
        public TaskMetaInformation[] TaskMetas { get; set; }
    }
}