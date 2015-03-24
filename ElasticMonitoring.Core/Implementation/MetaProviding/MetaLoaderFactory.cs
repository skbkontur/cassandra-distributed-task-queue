using RemoteQueue.Cassandra.Repositories;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding
{
    public class MetaLoaderFactory : IMetaLoaderFactory
    {
        public MetaLoaderFactory(IEventLogRepository eventLogRepository, IHandleTasksMetaStorage handleTasksMetaStorage)
        {
            this.eventLogRepository = eventLogRepository;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
        }

        public IMetasLoader CreateLoader(string name)
        {
            return new MetaLoader(eventLogRepository, handleTasksMetaStorage, name);
        }

        private readonly IEventLogRepository eventLogRepository;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
    }
}