using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IHandleTasksMetaStorage
    {
        [NotNull]
        IEnumerable<TaskIndexRecord> GetIndexRecords(long toTicks, [NotNull] params TaskState[] states);

        [NotNull]
        TaskIndexRecord AddMeta([NotNull] TaskMetaInformation taskMeta);

        TaskMetaInformation GetMeta(string taskId);
        TaskMetaInformation[] GetMetas(string[] taskIds);
        TaskMetaInformation[] GetMetasQuiet(string[] taskIds);
    }
}