using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IHandleTasksMetaStorage
    {
        IEnumerable<Tuple<string, TaskColumnInfo>> GetAllTasksInStates(long toTicks, params TaskState[] states);

        [NotNull]
        TaskColumnInfo AddMeta([NotNull] TaskMetaInformation taskMeta);

        TaskMetaInformation GetMeta(string taskId);
        TaskMetaInformation[] GetMetas(string[] taskIds);
        TaskMetaInformation[] GetMetasQuiet(string[] taskIds);
    }
}