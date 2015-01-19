using GroboContainer.Core;

using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.RemoteTaskQueue.Storage;
using SKBKontur.Catalogue.ServiceLib;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Configuration
{
    public static class Configurator
    {
        public static void Configure(IContainer container)
        {
            container.Configurator.ForAbstraction<IAuthorizationStrategy>().UseType<EmptyAuthorizationStrategy>();
            container.Get<IStorageConfigurator>().Configure(container);
            container.Get<RemoteTaskQueueMonitoringSchemaConfiguration>().ConfigureBusinessObjectStorage(container);
        }
    }
}