using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters
{
    public interface IMetaConsumer
    {
        void NewMetainformationAvailable(TaskMetaInformation[] metas, long readTicks);
    }
}