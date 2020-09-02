using System;

using JetBrains.Annotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerSettings
    {
        public RtqElasticsearchIndexerSettings([NotNull] string eventFeedKey, [NotNull] string rtqGraphitePathPrefix)
        {
            if (string.IsNullOrEmpty(eventFeedKey))
                throw new InvalidOperationException("eventFeedKey is empty");
            if (string.IsNullOrEmpty(rtqGraphitePathPrefix))
                throw new InvalidOperationException("rtqGraphitePathPrefix is empty");

            EventFeedKey = eventFeedKey;
            RtqGraphitePathPrefix = rtqGraphitePathPrefix;
        }

        [NotNull]
        public string EventFeedKey { get; }

        [NotNull]
        public string RtqGraphitePathPrefix { get; }

        [NotNull]
        public string PerfGraphitePathPrefix => $"{RtqGraphitePathPrefix}.ElasticsearchIndexer.Perf";

        [NotNull]
        public string EventFeedGraphitePathPrefix => $"{RtqGraphitePathPrefix}.ElasticsearchIndexer.EventFeed";

        [NotNull]
        public Timestamp InitialIndexingStartTimestamp { get; set; } = new Timestamp(new DateTime(2020, 01, 01, 0, 0, 0, DateTimeKind.Utc));

        public TimeSpan MaxEventsProcessingTimeWindow { get; set; } = TimeSpan.FromHours(1);

        public int MaxEventsProcessingTasksCount { get; set; } = 60000;

        public int TaskIdsProcessingBatchSize { get; set; } = 4000;

        public int IndexingThreadsCount { get; set; } = 2;

        public TimeSpan BulkIndexRequestTimeout { get; set; } = TimeSpan.FromMinutes(5);

        [NotNull]
        public JsonSerializerSettings JsonSerializerSettings { get; } = DefaultJsonSerializerSettings;

        public override string ToString()
        {
            return $"InitialIndexingStartTimestamp: {InitialIndexingStartTimestamp}, " +
                   $"MaxEventsProcessingTimeWindow: {MaxEventsProcessingTimeWindow}, " +
                   $"MaxEventsProcessingTasksCount: {MaxEventsProcessingTasksCount}, " +
                   $"TaskIdsProcessingBatchSize: {TaskIdsProcessingBatchSize}, " +
                   $"IndexingThreadsCount: {IndexingThreadsCount}, " +
                   $"BulkIndexRequestTimeout: {BulkIndexRequestTimeout}";
        }

        [NotNull]
        public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new OmitBinaryAndAbstractPropertiesContractResolver(),
                Converters = new JsonConverter[]
                    {
                        new TruncateLongStringsConverter(500),
                        new StringEnumConverter(),
                        new TimestampJsonConverter(),
                        new TimeGuidJsonConverter()
                    },
            };
    }
}