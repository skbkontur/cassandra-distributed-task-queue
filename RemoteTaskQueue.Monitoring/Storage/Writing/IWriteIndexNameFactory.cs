using RemoteQueue.Cassandra.Entities;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public interface IWriteIndexNameFactory
    {
        string GetIndexForTask(TaskMetaInformation taskMetaInformation);
    }
}