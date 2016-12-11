using System.Collections.Generic;

namespace RemoteTaskQueue.TaskCounter.Implementation.OldWaitingTasksCounters
{
    public class OldWaitingCounterSnapshot
    {
        public string[] Tasks { get; set; }
        public Dictionary<string, long> NotCountedNewTasks { get; set; }
    }
}