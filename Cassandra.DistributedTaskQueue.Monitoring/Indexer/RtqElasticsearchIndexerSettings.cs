#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer;

public class RtqElasticsearchIndexerSettings
{
    public RtqElasticsearchIndexerSettings(string eventFeedKey, string rtqGraphitePathPrefix)
    {
        if (string.IsNullOrEmpty(eventFeedKey))
            throw new InvalidOperationException("eventFeedKey is empty");
        if (string.IsNullOrEmpty(rtqGraphitePathPrefix))
            throw new InvalidOperationException("rtqGraphitePathPrefix is empty");

        EventFeedKey = eventFeedKey;
        RtqGraphitePathPrefix = rtqGraphitePathPrefix;
    }

    public string EventFeedKey { get; }

    public string RtqGraphitePathPrefix { get; }

    public string PerfGraphitePathPrefix => $"{RtqGraphitePathPrefix}.ElasticsearchIndexer.Perf";

    public string EventFeedGraphitePathPrefix => $"{RtqGraphitePathPrefix}.ElasticsearchIndexer.EventFeed";

    public Timestamp InitialIndexingStartTimestamp { get; } = new(new DateTime(2020, 01, 01, 0, 0, 0, DateTimeKind.Utc));

    public TimeSpan MaxEventsProcessingTimeWindow { get; } = TimeSpan.FromHours(1);

    public int MaxEventsProcessingTasksCount { get; set; } = 60000;

    public int TaskIdsProcessingBatchSize { get; set; } = 4000;

    public int IndexingThreadsCount { get; set; } = 2;

    public TimeSpan BulkIndexRequestTimeout { get; set; } = TimeSpan.FromMinutes(5);

    public TimeSpan InitialIndexingOffsetFromNow { get; set; } = TimeSpan.FromMinutes(30);

    public override string ToString()
    {
        return $"InitialIndexingStartTimestamp: {InitialIndexingStartTimestamp}, " +
               $"MaxEventsProcessingTimeWindow: {MaxEventsProcessingTimeWindow}, " +
               $"MaxEventsProcessingTasksCount: {MaxEventsProcessingTasksCount}, " +
               $"TaskIdsProcessingBatchSize: {TaskIdsProcessingBatchSize}, " +
               $"IndexingThreadsCount: {IndexingThreadsCount}, " +
               $"BulkIndexRequestTimeout: {BulkIndexRequestTimeout}";
    }
    
    public static readonly JsonSerializerOptions DefaultJsonSerializerSettings = new()
        {
            Converters =
                {
                    new TruncateLongStringsConverter(500),
                    new JsonStringEnumConverter(),
                    new TimestampJsonConverter(),
                    new TimeGuidJsonConverter(),
                    new OmitBinaryAndAbstractPropertyConverter()
                }
        };
}