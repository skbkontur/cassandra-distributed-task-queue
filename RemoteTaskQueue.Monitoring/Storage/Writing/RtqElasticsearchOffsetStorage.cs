using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public class RtqElasticsearchOffsetStorage : ElasticsearchOffsetStorage<string>
    {
        public RtqElasticsearchOffsetStorage(RtqElasticsearchClientFactory elasticsearchClientFactory, [NotNull] string bladeKey)
            : base(elasticsearchClientFactory.DefaultClient.Value, key : bladeKey, indexName : RtqElasticsearchConsts.IndexingProgressIndex)
        {
        }

        [NotNull]
        protected override sealed string GetDefaultOffset()
        {
            return "now - 42 days";
        }
    }
}