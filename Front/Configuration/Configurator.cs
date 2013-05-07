using GroboContainer.Core;

using SKBKontur.Catalogue.RemoteTaskQueue.Storage;
using SKBKontur.Catalogue.ServiceLib;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Configuration
{
    public static class Configurator
    {
        public static void Configure(IContainer container)
        {
            container.Get<IStorageConfigurator>().Configure(container);
            container.Get<RemoteTaskQueueMonitoringSchemaConfiguration>().ConfigureBusinessObjectStorage(container);
        }
    }
}