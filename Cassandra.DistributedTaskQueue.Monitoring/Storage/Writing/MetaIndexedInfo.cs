using Newtonsoft.Json;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Utils;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing
{
    public class MetaIndexedInfo
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string State { get; set; }
        public string ParentTaskId { get; set; }
        public string TaskGroupLock { get; set; }
        public int Attempts { get; set; }

        [JsonConverter(typeof(UtcTicksJsonConverter))]
        public long EnqueueTime { get; set; }

        [JsonConverter(typeof(UtcTicksJsonConverter))]
        public long MinimalStartTime { get; set; }

        [JsonConverter(typeof(UtcTicksJsonConverter))]
        public long? StartExecutingTime { get; set; }

        [JsonConverter(typeof(UtcTicksJsonConverter))]
        public long? FinishExecutingTime { get; set; }

        [JsonConverter(typeof(UtcTicksJsonConverter))]
        public long LastModificationTime { get; set; }

        [JsonConverter(typeof(UtcTicksJsonConverter))]
        public long ExpirationTime { get; set; }

        public double? LastExecutionDurationInMs { get; set; }
    }
}