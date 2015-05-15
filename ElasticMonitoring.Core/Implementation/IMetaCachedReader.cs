using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public interface IMetaCachedReader
    {
        TaskMetaInformation[] ReadActualMetasQuiet(TaskMetaUpdatedEvent[] events, long nowTicks);
    }
}