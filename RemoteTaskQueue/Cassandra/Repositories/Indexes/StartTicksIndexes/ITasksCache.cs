using System;
using System.Collections.Generic;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    internal interface ITasksCache
    {
        IEnumerable<Tuple<string, ColumnInfo>> PassThroughtCache(IEnumerable<Tuple<string, ColumnInfo>> enumerable);
        bool Remove(string taskId);
    }
}