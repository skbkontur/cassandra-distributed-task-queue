using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities
{
    public class MonitoringSearchRequest : BusinessObject
    {
        public TaskState[] TaskStates { get; set; }
        public string[] TaskNames { get; set; }
        public string TaskId { get; set; }
        public string ParentTaskId { get; set; }

        public DateTimeRange Ticks { get; set; }
        public DateTimeRange MinimalStartTicks { get; set; }
        public DateTimeRange StartExecutingTicks { get; set; }
        public DateTimeRange FinishExecutingTicks { get; set; }
    }
}