using System.Collections.Generic;

using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface ITaskMinimalStartTicksIndex
    {
        [CanBeNull]
        LiveRecordTicksMarkerState TryGetCurrentLiveRecordTicksMarker([NotNull] TaskTopicAndState taskTopicAndState);

        void AddRecord([NotNull] TaskIndexRecord taskIndexRecord);

        void RemoveRecord([NotNull] TaskIndexRecord taskIndexRecord);

        [NotNull]
        IEnumerable<TaskIndexRecord> GetRecords([NotNull] TaskTopicAndState taskTopicAndState, long toTicks, int batchSize);
    }
}