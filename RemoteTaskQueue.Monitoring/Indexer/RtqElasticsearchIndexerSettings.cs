using System;

using JetBrains.Annotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing;
using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerSettings
    {
        public RtqElasticsearchIndexerSettings([NotNull] string eventFeedKey, [NotNull] string perfGraphitePathPrefix)
        {
            if (string.IsNullOrEmpty(eventFeedKey))
                throw new InvalidProgramStateException("eventFeedKey is empty");
            if (string.IsNullOrEmpty(perfGraphitePathPrefix))
                throw new InvalidProgramStateException("perfGraphitePathPrefix is empty");
            EventFeedKey = eventFeedKey;
            PerfGraphitePathPrefix = perfGraphitePathPrefix;
        }

        [NotNull]
        public string EventFeedKey { get; }

        [NotNull]
        public string PerfGraphitePathPrefix { get; }

        [NotNull]
        public Timestamp InitialIndexingStartTimestamp { get; set; } = new Timestamp(new DateTime(2016, 02, 01, 0, 0, 0, DateTimeKind.Utc));

        public TimeSpan MaxEventsProcessingTimeWindow { get; set; } = TimeSpan.FromHours(1);

        public int MaxEventsProcessingTasksCount { get; set; } = 60000; // slightly less than TaskIdsProcessingBatchSize * IndexingThreadsCount

        public int TaskIdsProcessingBatchSize { get; set; } = 4000;

        public int IndexingThreadsCount { get; set; } = 16;

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