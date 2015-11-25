using System.Collections.Generic;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.OldWaitingTasksCounters;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation
{
    public class CompositeCounterSnapshot
    {
        public Dictionary<string, ProcessedTasksCounter.CounterSnapshot> Snapshots { get; set; }
        public ProcessedTasksCounter.CounterSnapshot TotalSnapshot { get; set; }
        public OldWaitingCounterSnapshot OldWaitingCounterSnapshot { get; set; }
    }
}