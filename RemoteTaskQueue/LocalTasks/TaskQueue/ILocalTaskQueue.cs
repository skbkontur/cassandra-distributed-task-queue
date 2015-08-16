using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Handling;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public interface ILocalTaskQueue
    {
        void Start();
        void StopAndWait(int timeout = 10000);
        long GetQueueLength();
        void QueueTask(ColumnInfo taskInfo, TaskMetaInformation taskMeta, long nowTicks, TaskQueueReason taskQueueReason);
    }
}