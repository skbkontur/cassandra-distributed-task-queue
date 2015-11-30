using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IHandleTasksMetaStorage
    {
        [CanBeNull]
        LiveRecordTicksMarkerState TryGetCurrentLiveRecordTicksMarker([NotNull] TaskIndexShardKey taskIndexShardKey);

        [NotNull]
        TaskIndexRecord[] GetIndexRecords(long toTicks, [NotNull] params TaskIndexShardKey[] taskIndexShardKeys);

        [NotNull]
        TaskIndexRecord AddMeta([NotNull] TaskMetaInformation taskMeta);

        TaskMetaInformation GetMeta(string taskId);
        IDictionary<string, TaskMetaInformation> GetMetas(string[] taskIds);
    }
}