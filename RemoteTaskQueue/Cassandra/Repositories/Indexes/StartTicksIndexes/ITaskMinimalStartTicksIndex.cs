using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface ITaskMinimalStartTicksIndex
    {
        [NotNull]
        TaskColumnInfo IndexMeta([NotNull] TaskMetaInformation taskMeta);

        void UnindexMeta([NotNull] TaskColumnInfo taskColumnInfo);

        [NotNull]
        IEnumerable<Tuple<string, TaskColumnInfo>> GetTaskIds(TaskState taskState, long toTicks, int batchSize);
    }
}