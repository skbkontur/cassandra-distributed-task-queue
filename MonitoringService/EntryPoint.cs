using GroboTrace;

using RemoteQueue.Settings;

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
        protected override string ConfigFileName { get { return "monitoringService.csf"; } }

        private static void Main()
        {
            new EntryPoint().Run();
        }

        private void Run()
        {
            Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseType<RemoteQueueTestsCassandraSettings>();
            Container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseType<RemoteQueueTestsCassandraSettings>();
            Container.Get<ILocalStorage>().ActualizeDatabaseScheme();
            Container.Get<IMonitoringServiceSchedulableRunner>().Start();
            Container.Get<HttpService>().Run();
        }
    }
}