﻿using System.Text.Json.Serialization;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

public class RtqMonitoringTaskMeta
{
    [NotNull]
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [NotNull]
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("ticks")]
    [JsonConverter(typeof(LongToStringConverter))]
    public long Ticks { get; set; }

    [JsonPropertyName("minimalStartTicks")]
    [JsonConverter(typeof(LongToStringConverter))]
    public long MinimalStartTicks { get; set; }

    [JsonPropertyName("startExecutingTicks")]
    [JsonConverter(typeof(NullableLongToStringConverter))]
    public long? StartExecutingTicks { get; set; }

    [JsonPropertyName("finishExecutingTicks")]
    [JsonConverter(typeof(NullableLongToStringConverter))]
    public long? FinishExecutingTicks { get; set; }

    [JsonPropertyName("lastModificationTicks")]
    [JsonConverter(typeof(NullableLongToStringConverter))]
    public long? LastModificationTicks { get; set; }

    [JsonPropertyName("expirationTimestampTicks")]
    [JsonConverter(typeof(NullableLongToStringConverter))]
    public long? ExpirationTimestampTicks { get; set; }

    [JsonPropertyName("expirationModificationTicks")]
    [JsonConverter(typeof(NullableLongToStringConverter))]
    public long? ExpirationModificationTicks { get; set; }

    [JsonPropertyName("executionDurationTicks")]
    [JsonConverter(typeof(NullableLongToStringConverter))]
    public long? ExecutionDurationTicks { get; set; }

    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskState State { get; set; }

    [CanBeNull]
    [JsonPropertyName("taskActions")]
    public TaskActions TaskActions { get; set; }

    [JsonPropertyName("attempts")]
    public int Attempts { get; set; }

    [JsonPropertyName("parentTaskId")]
    public string ParentTaskId { get; set; }
}