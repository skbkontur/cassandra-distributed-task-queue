using RemoteQueue.Cassandra.Repositories;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding
{
    public class MetaLoader : IMetasLoader
    {
        public MetaLoader(IEventLogRepository eventLogRepository, IHandleTasksMetaStorage handleTasksMetaStorage, string name)
        {
            metaProviderImpl = new MetaProviderImpl(MetaProviderSettings.EventGarbageCollectionTimeout.Ticks, MetaProviderSettings.MaxBatch, 0, eventLogRepository, handleTasksMetaStorage, name + "Loader");
        }

        public void Reset(long startTicks)
        {
            metaProviderImpl.Restart(startTicks);
        }

        public void CancelLoadingAsync()
        {
            metaProviderImpl.CancelLoading();
        }

        public void Load(IMetaConsumer metaConsumer, long endTicks)
        {
            metaProviderImpl.LoadMetas(endTicks, new[] {metaConsumer});
        }

        private readonly MetaProviderImpl metaProviderImpl;
    }
}