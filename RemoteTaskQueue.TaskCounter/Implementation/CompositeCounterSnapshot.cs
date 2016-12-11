using System.Collections.Generic;

using RemoteTaskQueue.TaskCounter.Implementation.OldWaitingTasksCounters;

namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public class CompositeCounterSnapshot
    {
        public Dictionary<string, ProcessedTasksCounter.CounterSnapshot> Snapshots { get; set; }
        public ProcessedTasksCounter.CounterSnapshot TotalSnapshot { get; set; }
        public OldWaitingCounterSnapshot OldWaitingCounterSnapshot { get; set; }
    }
}