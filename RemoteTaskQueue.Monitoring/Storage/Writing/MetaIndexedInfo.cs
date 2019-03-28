using Newtonsoft.Json;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Serialization;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public class MetaIndexedInfo
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string State { get; set; }
        public string ParentTaskId { get; set; }
        public string TaskGroupLock { get; set; }
        public int Attempts { get; set; }

        [JsonConverter(typeof(UtcTicksDateConverter))]
        public long EnqueueTime { get; set; }

        [JsonConverter(typeof(UtcTicksDateConverter))]
        public long MinimalStartTime { get; set; }

        [JsonConverter(typeof(UtcTicksDateConverter))]
        public long? StartExecutingTime { get; set; }

        [JsonConverter(typeof(UtcTicksDateConverter))]
        public long? FinishExecutingTime { get; set; }

        [JsonConverter(typeof(UtcTicksDateConverter))]
        public long LastModificationTime { get; set; }

        [JsonConverter(typeof(UtcTicksDateConverter))]
        public long ExpirationTime { get; set; }

        public double? LastExecutionDurationInMs { get; set; }
    }
}