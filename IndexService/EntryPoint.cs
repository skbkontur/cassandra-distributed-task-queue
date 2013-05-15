using SKBKontur.Catalogue.CassandraStorageCore.Storage.BusinessObjects.Schema;
using SKBKontur.Catalogue.Core.IndexServiceCore;
using SKBKontur.Catalogue.Core.IndexServiceCore.Configuration;
using SKBKontur.Catalogue.Core.IndexServiceCore.LocalStorageRegistry;
using SKBKontur.Catalogue.RemoteTaskQueue.Storage;
using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.Services;

namespace SKBKontur.Catalogue.RemoteTaskQueue.IndexService
{
    public class EntryPoint : ApplicationBase
    {
        protected override string ConfigFileName { get { return "indexServiceSettings"; } }

        private static void Main()
        {
            new EntryPoint().Run();
        }

        private void Run()
        {
            Configurator.Configure(Container);


            var schemaConfigurator = Container.Get<RemoteTaskQueueMonitoringSchemaConfiguration>();
            schemaConfigurator.ConfigureBusinessObjectStorage(Container);

            var localStorageSchemaConfigurator = Container.Create<BusinessObjectStoringSchema, IndexServiceDefaultMySQLStorageSchemeConfigurator>(schemaConfigurator.BusinessObjectSchema);
            localStorageSchemaConfigurator.ConfigureSchema(Container);


            Container.Get<ISqlSettingsUpdater>().Start();
            Container.Get<IIndexCore>().ActualizeDatabaseScheme();
            Container.Get<IIndexSchedulableRunner>().Start();
            Container.Get<HttpService>().Run();
        }
    }
}