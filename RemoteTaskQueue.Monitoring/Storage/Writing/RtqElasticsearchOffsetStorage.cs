using System;

using JetBrains.Annotations;

using RemoteTaskQueue.Monitoring.Indexer;

using SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages;
using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public class RtqElasticsearchOffsetStorage : ElasticsearchOffsetStorage<string>
    {
        public RtqElasticsearchOffsetStorage(RtqElasticsearchClientFactory elasticsearchClientFactory, RtqMonitoringOffsetInterpreter offsetInterpreter, [NotNull] string bladeKey)
            : base(elasticsearchClientFactory.DefaultClient.Value, key : bladeKey, indexName : RtqElasticsearchConsts.IndexingProgressIndex)
        {
            this.offsetInterpreter = offsetInterpreter;
        }

        [NotNull]
        protected override sealed string GetDefaultOffset()
        {
            return offsetInterpreter.GetMaxOffsetForTimestamp(Timestamp.Now - TimeSpan.FromDays(42));
        }

        private readonly RtqMonitoringOffsetInterpreter offsetInterpreter;
    }
}