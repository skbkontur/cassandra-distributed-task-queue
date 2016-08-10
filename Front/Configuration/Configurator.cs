using GroboContainer.Core;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraStorageCore;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
using SKBKontur.Catalogue.RemoteTaskQueue.Storage;

using TestCommon;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Configuration
{
    public static class Configurator
    {
        public static void Configure(IContainer container)
        {
            container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseType<RemoteQueueTestsCassandraSettings>();
            container.ConfigureRemoteTaskQueue();
            container.Configurator.ForAbstraction<ICassandraCoreSettings>().UseInstances(new TestCassandraCoreSettings());
            container.Get<RemoteTaskQueueMonitoringSchemaConfiguration>().ConfigureBusinessObjectStorage(container);
        }
    }
}