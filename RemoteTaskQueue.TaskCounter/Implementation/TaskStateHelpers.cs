using System;

using RemoteQueue.Cassandra.Entities;

namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public class TaskStateHelpers
    {
        public static readonly int statesCount = Enum.GetValues(typeof(TaskState)).Length;
    }
}