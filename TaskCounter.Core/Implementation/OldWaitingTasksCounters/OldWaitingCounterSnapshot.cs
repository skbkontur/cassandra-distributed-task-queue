using System.Collections.Generic;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.OldWaitingTasksCounters
{
    public class OldWaitingCounterSnapshot
    {
        public string[] Tasks { get; set; }
        public Dictionary<string, long> NotCountedNewTasks { get; set; }
    }
}