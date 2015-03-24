using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding
{
    public interface IMetaConsumer
    {
        void ProcessMetas(TaskMetaInformation[] metas, long readTicks);
    }
}