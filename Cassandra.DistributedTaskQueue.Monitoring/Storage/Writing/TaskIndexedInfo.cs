#nullable enable

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing;

public class TaskIndexedInfo
{
    public TaskIndexedInfo()
    {
    }

    public TaskIndexedInfo(MetaIndexedInfo meta, string exceptionInfo, object? data)
    {
        Meta = meta;
        ExceptionInfo = exceptionInfo;
        if (data != null)
            Data = new Dictionary<string, object> {{meta.Name, data}};
    }

    public MetaIndexedInfo Meta { get; set; } = null!;

    [JsonConverter(typeof(TruncateLongStringsConverter2K))]
    public string ExceptionInfo { get; set; } = null!;

    // NOTE! Using TaskTypeName->TaskData dictionary here to avoid type conflicts between fields with the same name in different TaskData contracts
    // since we must index all TaskData types into single elasticsearch mapping type (see https://www.elastic.co/guide/en/elasticsearch/reference/current/removal-of-types.html)
    public Dictionary<string, object> Data { get; set; } = null!;
}