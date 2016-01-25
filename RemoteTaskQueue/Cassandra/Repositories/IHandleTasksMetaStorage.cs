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
        TaskIndexRecord AddMeta([NotNull] TaskMetaInformation taskMeta, [CanBeNull] TaskIndexRecord oldTaskIndexRecord);

        [NotNull]
        TaskIndexRecord FormatIndexRecord([NotNull] TaskMetaInformation taskMeta);

        [NotNull]
        TaskMetaInformation GetMeta([NotNull] string taskId);

        [NotNull]
        Dictionary<string, TaskMetaInformation> GetMetas([NotNull] string[] taskIds);
    }
}