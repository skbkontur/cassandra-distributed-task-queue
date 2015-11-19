using System;

using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation
{
    public class TaskStateHelpers
    {
        public static readonly int statesCount = Enum.GetValues(typeof(TaskState)).Length;
    }
}