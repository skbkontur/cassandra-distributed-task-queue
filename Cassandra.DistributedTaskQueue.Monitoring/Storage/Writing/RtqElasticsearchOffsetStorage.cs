#nullable disable

using System;
using System.Text.Json;

using Elasticsearch.Net;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Get;
using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing;

public class RtqElasticsearchOffsetStorage : IOffsetStorage<string>
{
    public RtqElasticsearchOffsetStorage(IRtqElasticsearchClient elasticsearchClient,
                                         RtqEventLogOffsetInterpreter offsetInterpreter,
                                         string bladeKey,
                                         TimeSpan initialIndexingOffsetFromNow)
    {
        this.elasticsearchClient = elasticsearchClient;
        this.offsetInterpreter = offsetInterpreter;
        this.bladeKey = bladeKey;
        this.initialIndexingOffsetFromNow = initialIndexingOffsetFromNow;
    }

    public string GetDescription()
    {
        return $"RtqElasticsearchOffsetStorage with IndexName: {elasticIndexName}, BladeKey: {bladeKey}";
    }

    public void Write(string newOffset)
    {
        var payload = new OffsetStorageElement {Offset = newOffset};
        var postData = PostData.String(JsonSerializer.Serialize(payload));

        if (elasticsearchClient.UseElastic7)
            elasticsearchClient.Index<StringResponse>(elasticIndexName, bladeKey, postData).EnsureSuccess();
        else
#pragma warning disable CS0618
            elasticsearchClient.IndexUsingType<StringResponse>(elasticIndexName, elasticTypeName, bladeKey, postData).EnsureSuccess();
#pragma warning restore CS0618
    }

    public string Read()
    {
        var stringResponse = elasticsearchClient.UseElastic7
                                 ? elasticsearchClient.Get<StringResponse>(elasticIndexName, bladeKey, allowNotFoundStatusCode).EnsureSuccess()
#pragma warning disable CS0618
                                 : elasticsearchClient.GetUsingType<StringResponse>(elasticIndexName, elasticTypeName, bladeKey, allowNotFoundStatusCode).EnsureSuccess();
#pragma warning restore CS0618
        if (string.IsNullOrEmpty(stringResponse.Body))
            return GetDefaultOffset();

        var elasticResponse = JsonSerializer.Deserialize<GetResponse<OffsetStorageElement>>(stringResponse.Body);
        if (elasticResponse?.Source == null || !elasticResponse.Found)
            return GetDefaultOffset();

        return elasticResponse.Source.Offset;
    }

    private string GetDefaultOffset()
    {
        return offsetInterpreter.GetMaxOffsetForTimestamp(Timestamp.Now - initialIndexingOffsetFromNow);
    }

    private const string elasticIndexName = RtqElasticsearchConsts.IndexingProgressIndexName;
    private const string elasticTypeName = "MultiRazorEventFeedOffset";

    private readonly IRtqElasticsearchClient elasticsearchClient;
    private readonly RtqEventLogOffsetInterpreter offsetInterpreter;
    private readonly string bladeKey;
    private readonly TimeSpan initialIndexingOffsetFromNow;

    private readonly GetRequestParameters allowNotFoundStatusCode = new()
        {
            RequestConfiguration = new RequestConfiguration {AllowedStatusCodes = new[] {404}}
        };

    private class OffsetStorageElement
    {
        public string Offset { get; set; }
    }
}