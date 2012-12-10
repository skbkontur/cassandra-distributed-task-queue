using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IHandleTasksMetaStorage
    {
        IEnumerable<string> GetAllTasksInStates(long ticks, params TaskState[] states);
        IEnumerable<string> GetReverseAllTasksInStatesOrder(long ticks, params TaskState[] states);
        IEnumerable<string> GetAllTasksInStatesFromTicks(long fromTicks, params TaskState[] states);
        void AddMeta(TaskMetaInformation meta);
        TaskMetaInformation GetMeta(string taskId);
    }
}