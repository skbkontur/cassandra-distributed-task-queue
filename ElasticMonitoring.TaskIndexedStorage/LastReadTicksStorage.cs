using Elasticsearch.Net;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Get;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage
{
    public class LastReadTicksStorage
    {
        public LastReadTicksStorage(
            IElasticsearchClientFactory elasticsearchClientFactory)
        {
            elasticsearchClient = elasticsearchClientFactory.GetClient();
        }

        public long GetLastReadTicks()
        {
            var response = elasticsearchClient.Get<GetResponse<LastUpdateTicks>>(TaskSearchIndexSchema.LastUpdateTicksIndex, TaskSearchIndexSchema.LastUpdateTicksType, "LastUpdateTicks").ProcessResponse();
            if(!response.Response.Found || response.Response.Source == null)
                return 0;
            return response.Response.Source.Ticks;
        }

        public void SetLastReadTicks(long ticks)
        {
            elasticsearchClient.Index(TaskSearchIndexSchema.LastUpdateTicksIndex, TaskSearchIndexSchema.LastUpdateTicksType, "LastUpdateTicks", new LastUpdateTicks() {Ticks = ticks},
                                      parameters => parameters.Version(ticks).VersionType(VersionType.ExternalGte)).ProcessResponse();
        }

        private readonly IElasticsearchClient elasticsearchClient;

        private class LastUpdateTicks
        {
            public long Ticks { get; set; }
        }
    }
}