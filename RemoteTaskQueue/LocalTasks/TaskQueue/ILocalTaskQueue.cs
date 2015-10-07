using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Handling;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public interface ILocalTaskQueue
    {
        void Start();
        void StopAndWait(TimeSpan timeout);
        void QueueTask([NotNull] string taskId, [NotNull] ColumnInfo taskInfo, [CanBeNull] TaskMetaInformation taskMeta, TaskQueueReason taskQueueReason, out bool queueIsFull, out bool taskIsSentToThreadPool, bool taskIsBeingTraced);
    }
}