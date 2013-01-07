using System;
using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IHandleTasksMetaStorage
    {
        IEnumerable<Tuple<string, ColumnInfo>> GetAllTasksInStates(long ticks, params TaskState[] states);
        void AddMeta(TaskMetaInformation meta);
        TaskMetaInformation GetMeta(string taskId);
    }
}