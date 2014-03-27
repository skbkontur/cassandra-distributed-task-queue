using GroboContainer.Core;

using SKBKontur.Catalogue.CassandraStorageCore;
using SKBKontur.Catalogue.CassandraStorageCore.Initializing;
using SKBKontur.Catalogue.IndexService.Client;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;

namespace FunctionalTests
{
    public static class TestBaseExtensions
    {
        public static void ConfigureCassandra(this IContainer container)
        {
            new CassandraConfigurator().Configure(container);
        }

        public static void ActualizeAllBeforeTest(this IContainer container)
        {
            var cassandraSchemeActualizer = container.Get<ICassandraSchemeActualizer>();
            cassandraSchemeActualizer.AddNewColumnFamilies();
            container.Get<IIndexServiceClient>().ActualizeDatabaseScheme();
        }

        public static void ClearAllBeforeTest(this IContainer container)
        {
            var cassandraSchemeActualizer = container.Get<ICassandraSchemeActualizer>();
            cassandraSchemeActualizer.AddNewColumnFamilies();
            cassandraSchemeActualizer.TruncateAllColumnFamilies();
            container.Get<IExchangeServiceClient>().Stop();
            container.Get<IIndexServiceClient>().DeleteAllTables();
            container.Get<IIndexServiceClient>().ActualizeDatabaseScheme();
            container.Get<IExchangeServiceClient>().Start();
        }
    }
}