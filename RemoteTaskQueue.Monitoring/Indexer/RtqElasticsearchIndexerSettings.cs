using System;

using JetBrains.Annotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using RemoteTaskQueue.Monitoring.Storage.Writing;

using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.Json;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerSettings
    {
        public RtqElasticsearchIndexerSettings()
        {
            InitialIndexingStartTimestamp = new Timestamp(new DateTime(2016, 02, 01, 0, 0, 0, DateTimeKind.Utc));
            MaxEventsProcessingTimeWindow = TimeSpan.FromHours(1);
            MaxEventsProcessingTasksCount = 60000; // slightly less than TaskIdsProcessingBatchSize * IndexingThreadsCount
            TaskIdsProcessingBatchSize = 4000;
            IndexingThreadsCount = 16;
        }

        [NotNull]
        public Timestamp InitialIndexingStartTimestamp { get; }

        public TimeSpan MaxEventsProcessingTimeWindow { get; set; }
        public int MaxEventsProcessingTasksCount { get; set; }
        public int TaskIdsProcessingBatchSize { get; set; }
        public int IndexingThreadsCount { get; set; }

        [NotNull]
        public JsonSerializerSettings JsonSerializerSettings => new JsonSerializerSettings
            {
                ContractResolver = new OmitBinaryAndAbstractPropertiesContractResolver(),
                Converters = new JsonConverter[]
                    {
                        new TruncateLongStringsConverter(500),
                        new StringEnumConverter(),
                    },
            };

        public override string ToString()
        {
            return $"InitialIndexingStartTimestamp: {InitialIndexingStartTimestamp}, MaxEventsProcessingTimeWindow: {MaxEventsProcessingTimeWindow}, MaxEventsProcessingTasksCount: {MaxEventsProcessingTasksCount}, TaskIdsProcessingBatchSize: {TaskIdsProcessingBatchSize}, IndexingThreadsCount: {IndexingThreadsCount}";
        }
    }
}