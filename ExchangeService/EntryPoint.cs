using GroboTrace;

using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.Services;

namespace ExchangeService
{
    internal class EntryPoint : ApplicationBase
    {
        protected override void ConfigureTracingWrapper(TracingWrapperConfigurator configurator)
        {
        }

        protected override string ConfigFileName { get { return "exchangeService.csf"; } }

        private static void Main()
        {
            new EntryPoint().Run();
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