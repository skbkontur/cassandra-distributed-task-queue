using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface ITaskMinimalStartTicksIndex
    {
        IEnumerable<Tuple<string, ColumnInfo>> GetTaskIds(TaskState taskState, long toTicks, int batchSize);

        [NotNull]
        ColumnInfo IndexMeta([NotNull] TaskMetaInformation taskMetaInformation);

        void UnindexMeta(string taskId, ColumnInfo columnInfo);
    }
}