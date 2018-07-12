using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;

namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public interface ICompositeCounter
    {
        void ProcessMetas(TaskMetaInformation[] metas, long readTicks);
        TaskCount GetTotalCount();
        Dictionary<string, TaskCount> GetAllCounts();
        void Reset();
        CompositeCounterSnapshot GetSnapshotOrNull(int maxLength);
        void LoadSnapshot(CompositeCounterSnapshot snapshot);
    }
}