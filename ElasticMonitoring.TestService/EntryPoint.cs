using GroboContainer.Core;

using GroboTrace;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;
using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Scheduler;
using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.Services;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
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
            Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(Container.Get<RemoteQueueTestsCassandraSettings>());
            ConfigureRemoteLock(Container);
            Container.Get<IElasticMonitoringServiceSchedulableRunner>().Start();
            Container.Get<HttpService>().Run();
        }

        private static void ConfigureRemoteLock(IContainer container)
        {
            var keyspaceName = container.Get<IApplicationSettings>().GetString("KeyspaceName");
            const string columnFamilyName = "remoteLock";
            container.Configurator.ForAbstraction<IRemoteLockImplementation>().UseInstances(container.Create<ColumnFamilyFullName, CassandraRemoteLockImplementation>(new ColumnFamilyFullName(keyspaceName, columnFamilyName)));
        }
    }
}