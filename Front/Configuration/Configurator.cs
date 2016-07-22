using GroboContainer.Core;

using SKBKontur.Catalogue.RemoteTaskQueue.Storage;

using TestCommon;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Configuration
{
    public static class Configurator
    {
        public static void Configure(IContainer container)
        {
            container.ConfigureForTests();
            container.Get<RemoteTaskQueueMonitoringSchemaConfiguration>().ConfigureBusinessObjectStorage(container);
        }
    }
}