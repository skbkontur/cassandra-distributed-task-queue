using System;
using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface ITaskMinimalStartTicksIndex
    {
        IEnumerable<Tuple<string, ColumnInfo>> GetTaskIds(TaskState taskState, long nowTicks, int batchSize = 2000);
        void IndexMeta(TaskMetaInformation obj);
    }
}