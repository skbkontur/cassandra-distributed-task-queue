using System.Collections.Generic;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation
{
    public class CompositeCounterSnapshot
    {
        public Dictionary<string, ProcessedTasksCounter.CounterSnapshot> Snapshots { get; set; }
        public ProcessedTasksCounter.CounterSnapshot TotalSnapshot { get; set; }
    }
}