using RemoteQueue.Cassandra.Entities;

namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public interface IMetaCachedReader
    {
        TaskMetaInformation[] ReadActualMetasQuiet(TaskMetaUpdatedEvent[] events, long nowTicks);
    }
}