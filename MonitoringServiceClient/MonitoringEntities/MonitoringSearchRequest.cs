using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.MonitoringEntities
{
    public class MonitoringSearchRequest : BusinessObject
    {
        public TaskState[] States { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string ParentTaskId { get; set; }

        public DateTimeRange Ticks { get; set; }
        public DateTimeRange MinimalStartTicks { get; set; }
        public DateTimeRange StartExecutingTicks { get; set; }
    }
}