using System;

using Elasticsearch.Net;

using JetBrains.Annotations;

using Newtonsoft.Json;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions;
using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing
{
    public class RtqElasticsearchOffsetStorage : IOffsetStorage<string>
    {
        public RtqElasticsearchOffsetStorage(IRtqElasticsearchClient elasticsearchClient,
                                             RtqEventLogOffsetInterpreter offsetInterpreter,
                                             [NotNull] string bladeKey)
        {
            this.elasticsearchClient = elasticsearchClient;
            this.offsetInterpreter = offsetInterpreter;
            this.bladeKey = bladeKey;
        }

        [NotNull]
        public string GetDescription()
        {
            return $"RtqElasticsearchOffsetStorage with IndexName: {elasticIndexName}, ElasticType: {elasticTypeName}, BladeKey: {bladeKey}";
        }

        public void Write([CanBeNull] string newOffset)
        {
            var payload = new OffsetStorageElement {Offset = newOffset};
            var postData = PostData.String(JsonConvert.SerializeObject(payload));
            elasticsearchClient.Index<StringResponse>(elasticIndexName, elasticTypeName, bladeKey, postData).EnsureSuccess();
        }

        [CanBeNull]
        public string Read()
        {
            var stringResponse = elasticsearchClient.Get<StringResponse>(elasticIndexName, elasticTypeName, bladeKey, allowNotFoundStatusCode).EnsureSuccess();
            if (string.IsNullOrEmpty(stringResponse.Body))
                return GetDefaultOffset();

            var elasticResponse = JsonConvert.DeserializeObject<GetResponse<OffsetStorageElement>>(stringResponse.Body);
            if (elasticResponse?.Source == null || !elasticResponse.Found)
                return GetDefaultOffset();

            return elasticResponse.Source.Offset;
        }

        [CanBeNull]
        private string GetDefaultOffset()
        {
            return offsetInterpreter.GetMaxOffsetForTimestamp(Timestamp.Now - TimeSpan.FromDays(3));
        }

        private const string elasticIndexName = RtqElasticsearchConsts.IndexingProgressIndexName;
        private const string elasticTypeName = "MultiRazorEventFeedOffset";

        private readonly IElasticLowLevelClient elasticsearchClient;
        private readonly RtqEventLogOffsetInterpreter offsetInterpreter;
        private readonly string bladeKey;

        private readonly GetRequestParameters allowNotFoundStatusCode = new GetRequestParameters
            {
                RequestConfiguration = new RequestConfiguration {AllowedStatusCodes = new[] {404}}
            };

        private class OffsetStorageElement
        {
            public string Offset { get; set; }
        }
    }
}