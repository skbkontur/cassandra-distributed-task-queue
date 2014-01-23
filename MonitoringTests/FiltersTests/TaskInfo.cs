using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests
{
    public class TaskInfo
    {
        public string TaskId { get; set; }
        public DateTime? EnqueueTime { get; set; }
        public DateTime? MinimalStartTime { get; set; }
    }
}