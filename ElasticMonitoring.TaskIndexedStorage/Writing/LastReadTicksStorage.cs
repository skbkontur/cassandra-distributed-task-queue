using Elasticsearch.Net;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Get;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing.Contracts;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    public class LastReadTicksStorage
    {
        public LastReadTicksStorage(
            IElasticsearchClientFactory elasticsearchClientFactory, ITaskWriteDynamicSettings settings)
        {
            elasticsearchClient = elasticsearchClientFactory.GetClient();
            index = settings.LastTicksIndex;
        }

        public long GetLastReadTicks()
        {
            var response = elasticsearchClient.Get<GetResponse<LastUpdateTicks>>(index, lastUpdateTicksType, id).ProcessResponse();
            if(!response.Response.Found || response.Response.Source == null)
                return 0;
            return response.Response.Source.Ticks;
        }

        public void SetLastReadTicks(long ticks)
        {
            elasticsearchClient.Index(index, lastUpdateTicksType, id, new LastUpdateTicks {Ticks = ticks}).ProcessResponse();
        }

        private const string lastUpdateTicksType = "LastUpdateTicks";
        private const string id = "LastUpdateTicks";
        private readonly IElasticsearchClient elasticsearchClient;
        private readonly string index;
    }
}