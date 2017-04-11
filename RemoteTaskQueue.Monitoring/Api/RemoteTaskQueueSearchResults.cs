using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.InternalApi.Core;

namespace RemoteTaskQueue.Monitoring.Api
{
    [InternalAPI]
    public class RemoteTaskQueueSearchResults
    {
        public long TotalCount { get; set; }

        public TaskMetaInformation[] TaskMetas { get; set; }
    }
}