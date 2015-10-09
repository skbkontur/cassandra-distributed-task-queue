using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface ITaskMinimalStartTicksIndex
    {
        [NotNull]
        TaskIndexRecord AddRecord([NotNull] TaskMetaInformation taskMeta);

        void RemoveRecord([NotNull] TaskIndexRecord taskIndexRecord);

        [NotNull]
        IEnumerable<TaskIndexRecord> GetRecords(TaskState taskState, long toTicks, int batchSize);
    }
}