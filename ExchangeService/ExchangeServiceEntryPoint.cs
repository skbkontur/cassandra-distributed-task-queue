using RemoteQueue.Settings;

using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.Services;

namespace ExchangeService
{
    public class ExchangeServiceEntryPoint : ApplicationBase
    {
        protected override string ConfigFileName { get { return "exchangeService.csf"; } }

        private static void Main()
        {
            new ExchangeServiceEntryPoint().Run();
        }

        private void Run()
        {
            Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseType<RemoteQueueTestsCassandraSettings>();
            Container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseType<RemoteQueueTestsCassandraSettings>();
            Container.ConfigureLockRepository();
            Container.Get<HttpService>().Run();
        }
    }
}