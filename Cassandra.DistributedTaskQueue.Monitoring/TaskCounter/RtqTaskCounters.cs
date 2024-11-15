#nullable enable

using System.Collections.Generic;
using System.Linq;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter;

public class RtqTaskCounters
{
    public int LostTasksCount { get; set; }

    public Dictionary<TaskState, int> PendingTaskCountsTotal { get; set; } = null!;

    public Dictionary<(string TaskName, string TaskTopic), Dictionary<TaskState, int>> PendingTaskCountsByNameAndTopic { get; set; } = null!;

    public int GetPendingTaskTotalCount()
    {
        return PendingTaskCountsTotal.Values.Sum();
    }
}