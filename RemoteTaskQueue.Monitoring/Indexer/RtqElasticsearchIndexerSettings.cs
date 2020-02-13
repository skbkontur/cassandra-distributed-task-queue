using System;

using JetBrains.Annotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing;
using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Objects.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerSettings
    {
        [NotNull]
        public Timestamp InitialIndexingStartTimestamp { get; set; } = new Timestamp(new DateTime(2016, 02, 01, 0, 0, 0, DateTimeKind.Utc));

        public TimeSpan MaxEventsProcessingTimeWindow { get; set; } = TimeSpan.FromHours(1);

        public int MaxEventsProcessingTasksCount { get; set; } = 60000; // slightly less than TaskIdsProcessingBatchSize * IndexingThreadsCount

        public int TaskIdsProcessingBatchSize { get; set; } = 4000;

        public int IndexingThreadsCount { get; set; } = 16;

        public TimeSpan BulkIndexRequestTimeout { get; set; } = TimeSpan.FromMinutes(5);

        [NotNull]
        public JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings
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

        public override string ToString()
        {
            return $"InitialIndexingStartTimestamp: {InitialIndexingStartTimestamp}, MaxEventsProcessingTimeWindow: {MaxEventsProcessingTimeWindow}, MaxEventsProcessingTasksCount: {MaxEventsProcessingTasksCount}, TaskIdsProcessingBatchSize: {TaskIdsProcessingBatchSize}, IndexingThreadsCount: {IndexingThreadsCount}, BulkIndexRequestTimeout: {BulkIndexRequestTimeout}";
        }
    }
}