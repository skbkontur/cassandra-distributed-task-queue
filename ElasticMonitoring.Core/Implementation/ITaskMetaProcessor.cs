using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public interface ITaskMetaProcessor
    {
        void IndexMetas(TaskMetaInformation[] batch);
    }
}