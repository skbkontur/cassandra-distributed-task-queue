using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public interface ITaskMetaProcessor
    {
        void IndexMetas([NotNull] TaskMetaInformation[] batch);
    }
}