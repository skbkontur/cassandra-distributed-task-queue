using System.Collections.Generic;

using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface ITaskMinimalStartTicksIndex
    {
        void AddRecord([NotNull] TaskIndexRecord taskIndexRecord);

        void RemoveRecord([NotNull] TaskIndexRecord taskIndexRecord);

        [NotNull]
        IEnumerable<TaskIndexRecord> GetRecords([NotNull] TaskNameAndState taskNameAndState, long toTicks, int batchSize);
    }
}