using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter
{
    public class RtqTaskCounters
    {
        public int LostTasksCount { get; set; }

        [NotNull]
        public Dictionary<TaskState, int> PendingTaskCountsTotal { get; set; }

        [NotNull]
        public Dictionary<string, Dictionary<TaskState, int>> PendingTaskCountsByName { get; set; }

        public int GetPendingTaskTotalCount()
        {
            return PendingTaskCountsTotal.Values.Sum();
        }
    }
}