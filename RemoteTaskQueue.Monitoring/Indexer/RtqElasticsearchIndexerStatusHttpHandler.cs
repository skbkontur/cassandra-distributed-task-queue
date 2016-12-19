using SKBKontur.Catalogue.Objects.Json;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerStatusHttpHandler : IHttpHandler
    {
        public RtqElasticsearchIndexerStatusHttpHandler(IRtqElasticsearchIndexer indexer)
        {
            this.indexer = indexer;
        }

        [HttpMethod]
        public RtqElasticsearchIndexerStatus GetStatus()
        {
            return indexer.GetStatus();
        }

        [HttpMethod]
        [JsonHttpMethod]
        public string GetStatusJson()
        {
            return indexer.GetStatus().ToJson();
        }

        private readonly IRtqElasticsearchIndexer indexer;
    }
}