using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IHandleTasksMetaStorage
    {
        IEnumerable<Tuple<string, ColumnInfo>> GetAllTasksInStates(long ticks, params TaskState[] states);

        [NotNull]
        ColumnInfo AddMeta([NotNull] TaskMetaInformation meta);

        TaskMetaInformation GetMeta(string taskId);
        TaskMetaInformation[] GetMetas(string[] taskIds);
        TaskMetaInformation[] GetMetasQuiet(string[] taskIds);
    }
}