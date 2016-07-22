using GroboContainer.Core;

using SKBKontur.Catalogue.CassandraStorageCore;
using SKBKontur.Catalogue.RemoteTaskQueue.Storage;

using TestCommon;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Configuration
{
    public static class Configurator
    {
        public static void Configure(IContainer container)
        {
            WithTestRemoteTaskQueue.SetUpRemoteTaskQueue(container);
            container.Configurator.ForAbstraction<ICassandraCoreSettings>().UseInstances(new TestCassandraCoreSettings());
            container.Get<RemoteTaskQueueMonitoringSchemaConfiguration>().ConfigureBusinessObjectStorage(container);
        }
    }
}