using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface ITaskMinimalStartTicksIndex
    {
        IEnumerable<string> GetTaskIds(TaskState taskState, long nowTicks, long fromTicks, bool reverseOrder = false, int batchSize = 2000);
        void IndexMeta(TaskMetaInformation obj);
    }
}