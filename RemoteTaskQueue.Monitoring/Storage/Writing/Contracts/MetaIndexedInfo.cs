using Newtonsoft.Json;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Serialization;

namespace RemoteTaskQueue.Monitoring.Storage.Writing.Contracts
{
    public class MetaIndexedInfo
    {
        public string Name { get; set; }
        public string Id { get; set; }

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

        public string State { get; set; }
        public int Attempts { get; set; }

        public string ParentTaskId { get; set; }
        public string TaskGroupLock { get; set; }

        [JsonConverter(typeof(UtcTicksDateConverter))]
        public long ExpirationTime { get; set; }

        //TODO разобраться с форматом дат. сейчас "2015-02-12T12:48:14.5995995Z"
        //NOTE в этом формате ticks не портятся (dateOptionalTime in mapping)
    }
}