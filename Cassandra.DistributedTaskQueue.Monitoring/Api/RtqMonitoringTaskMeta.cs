using JetBrains.Annotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public class RtqMonitoringTaskMeta
    {
        [NotNull]
        [JsonProperty("name")]
        public string Name { get; set; }

        [NotNull]
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("ticks")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long Ticks { get; set; }

        [JsonProperty("minimalStartTicks")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long MinimalStartTicks { get; set; }

        [JsonProperty("startExecutingTicks")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long? StartExecutingTicks { get; set; }

        [JsonProperty("finishExecutingTicks")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long? FinishExecutingTicks { get; set; }

        [JsonProperty("lastModificationTicks")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long? LastModificationTicks { get; set; }

        [JsonProperty("expirationTimestampTicks")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long? ExpirationTimestampTicks { get; set; }

        [JsonProperty("expirationModificationTicks")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long? ExpirationModificationTicks { get; set; }

        [JsonProperty("executionDurationTicks")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long? ExecutionDurationTicks { get; set; }

        [JsonProperty("state")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TaskState State { get; set; }

        [CanBeNull]
        [JsonProperty("taskActions")]
        public TaskActions TaskActions { get; set; }

        [JsonProperty("attempts")]
        public int Attempts { get; set; }

        [JsonProperty("parentTaskId")]
        public string ParentTaskId { get; set; }
    }
}