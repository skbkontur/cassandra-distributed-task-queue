#nullable enable

using System.Collections.Generic;
using System.Linq;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests;

public class RtqTaskCountersForTests
{
    public int LostTasksCount { get; set; }

    public Dictionary<TaskState, int> PendingTaskCountsTotal { get; set; } = null!;

    public int GetPendingTaskTotalCount()
    {
        return PendingTaskCountsTotal.Values.Sum();
    }
}