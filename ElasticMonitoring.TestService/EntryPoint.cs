using GroboContainer.Core;

using GroboTrace;

using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock.RemoteLocker;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;
using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation;
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

        protected override string ConfigFileName { get { return "monitoringService.csf"; } }

        private static void Main()
        {
            new EntryPoint().Run();
        }

        private void Run()
        {
            Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(Container.Get<RemoteQueueTestsCassandraSettings>());
            ConfigureRemoteLock(Container);
            Container.Get<ElasticAvailabilityChecker>().WaitAlive(); 
            Container.Get<LazySchemaActualizer>().ActualizeSchema(); //NOTE hack for local usage
            Container.Get<IElasticMonitoringServiceSchedulableRunner>().Start();
            Container.Get<HttpService>().Run();
        }

        private static void ConfigureRemoteLock(IContainer container)
        {
            var keyspaceName = container.Get<ICassandraSettings>().QueueKeyspace;
            const string columnFamilyName = "remoteLock";
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(new ColumnFamilyFullName(keyspaceName, columnFamilyName));
            var remoteLockImplementation = container.Create<CassandraRemoteLockImplementationSettings, CassandraRemoteLockImplementation>(remoteLockImplementationSettings);
            container.Configurator.ForAbstraction<IRemoteLockCreator>().UseInstances(new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics(keyspaceName)));
        }
    }
}