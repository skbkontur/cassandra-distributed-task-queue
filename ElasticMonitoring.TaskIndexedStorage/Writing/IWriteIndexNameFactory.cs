using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    public interface IWriteIndexNameFactory
    {
        string GetIndexForTask(TaskMetaInformation taskMetaInformation);
    }
}