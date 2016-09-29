using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface ITaskMinimalStartTicksIndex
    {
        [CanBeNull]
        LiveRecordTicksMarkerState TryGetCurrentLiveRecordTicksMarker([NotNull] TaskIndexShardKey taskIndexShardKey);

        void AddRecord([NotNull] TaskIndexRecord taskIndexRecord, long timestamp, TimeSpan? ttl);

        void WriteRecord([NotNull] TaskIndexRecord taskIndexRecord, long timestamp, TimeSpan? ttl);

        void RemoveRecord([NotNull] TaskIndexRecord taskIndexRecord, long timestamp);

        [NotNull]
        IEnumerable<TaskIndexRecord> GetRecords([NotNull] TaskIndexShardKey taskIndexShardKey, long toTicks, int batchSize);
    }
}