using Elasticsearch.Net;

using JetBrains.Annotations;

using Newtonsoft.Json;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions;
using SkbKontur.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeed
{
    public class ElasticsearchOffsetStorage<TOffset> : IOffsetStorage<TOffset>
    {
        public ElasticsearchOffsetStorage([NotNull] IElasticLowLevelClient elasticClient, [NotNull] string key, [NotNull] string indexName = "event-feed-offsets")
        {
            this.elasticClient = elasticClient;
            this.indexName = indexName;
            this.key = key;
        }

        [NotNull]
        public string GetDescription()
        {
            return $"ElasticsearchOffsetStorage<{typeof(TOffset)}> with IndexName: {indexName}, ElasticType: {elasticTypeName}, Key: {key}";
        }

        public void Write([CanBeNull] TOffset newOffset)
        {
            var payload = new OffsetStorageElement {Offset = newOffset};
            var postData = PostData.String(JsonConvert.SerializeObject(payload));
            elasticClient.Index<StringResponse>(indexName, elasticTypeName, key, postData).EnsureSuccess();
        }

        [CanBeNull]
        public TOffset Read()
        {
            var stringResponse = elasticClient.Get<StringResponse>(indexName, elasticTypeName, key, allowNotFoundStatusCode).EnsureSuccess();
            if (string.IsNullOrEmpty(stringResponse.Body))
                return GetDefaultOffset();

            var elasticResponse = JsonConvert.DeserializeObject<GetResponse<OffsetStorageElement>>(stringResponse.Body);
            if (elasticResponse?.Source == null || !elasticResponse.Found)
                return GetDefaultOffset();

            return elasticResponse.Source.Offset;
        }

        [CanBeNull]
        protected virtual TOffset GetDefaultOffset()
        {
            return default;
        }

        private const string elasticTypeName = "MultiRazorEventFeedOffset";
        private readonly IElasticLowLevelClient elasticClient;
        private readonly string indexName;
        private readonly string key;

        private readonly GetRequestParameters allowNotFoundStatusCode = new GetRequestParameters
            {
                RequestConfiguration = new RequestConfiguration {AllowedStatusCodes = new[] {404}}
            };

        private class OffsetStorageElement
        {
            public TOffset Offset { get; set; }
        }
    }
}