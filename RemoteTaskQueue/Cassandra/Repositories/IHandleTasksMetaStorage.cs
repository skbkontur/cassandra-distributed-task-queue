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
        LiveRecordTicksMarkerState TryGetCurrentLiveRecordTicksMarker([NotNull] TaskTopicAndState taskTopicAndState);

        [NotNull]
        IEnumerable<TaskIndexRecord> GetIndexRecords(long toTicks, [NotNull] params TaskTopicAndState[] taskTopicAndStates);

        [NotNull]
        TaskIndexRecord AddMeta([NotNull] TaskMetaInformation taskMeta);

        TaskMetaInformation GetMeta(string taskId);
        TaskMetaInformation[] GetMetas(string[] taskIds);
        TaskMetaInformation[] GetMetasQuiet(string[] taskIds);
    }
}