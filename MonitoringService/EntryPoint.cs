using GroboTrace;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Sheduler;
using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.Services;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringService
{
    public class EntryPoint : ApplicationBase
    {
        protected override void ConfigureTracingWrapper(TracingWrapperConfigurator configurator)
        {
        }

        protected override string ConfigFileName { get { return "monitoringServiceSettings"; } }

        private static void Main()
        {
            new EntryPoint().Run();
        }

        private void Run()
        {
            Container.Get<ILocalStorage>().ActualizeDatabaseScheme();
            Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(Container.Get<CassandraSettings>());
            Container.Get<IMonitoringServiceSchedulableRunner>().Start();
            Container.Get<HttpService>().Run();
        }
    }
}