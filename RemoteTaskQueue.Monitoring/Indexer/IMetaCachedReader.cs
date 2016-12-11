using RemoteQueue.Cassandra.Entities;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public interface IMetaCachedReader
    {
        TaskMetaInformation[] ReadActualMetasQuiet(TaskMetaUpdatedEvent[] events, long nowTicks);
        void CollectGarbage(long nowTicks);
        long UnsafeGetCount();
    }
}