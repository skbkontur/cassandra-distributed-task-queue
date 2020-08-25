using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeed;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing
{
    public class RtqElasticsearchOffsetStorage : ElasticsearchOffsetStorage<string>
    {
        public RtqElasticsearchOffsetStorage(IRtqElasticsearchClient elasticsearchClient, RtqEventLogOffsetInterpreter offsetInterpreter, [NotNull] string bladeKey)
            : base(elasticsearchClient, key : bladeKey, indexName : RtqElasticsearchConsts.IndexingProgressIndexName)
        {
            this.offsetInterpreter = offsetInterpreter;
        }

        [NotNull]
        protected override sealed string GetDefaultOffset()
        {
            return offsetInterpreter.GetMaxOffsetForTimestamp(Timestamp.Now - TimeSpan.FromDays(3));
        }

        private readonly RtqEventLogOffsetInterpreter offsetInterpreter;
    }
}