using System.Linq;

using ExchangeService.UserClasses;

using GroboContainer.Core;

using RemoteQueue.Configuration;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraStorageCore.Initializing;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases;
using SKBKontur.Catalogue.RemoteTaskQueue.Storage;
using SKBKontur.Catalogue.TestCore;
using SKBKontur.Catalogue.WebTestCore;
using SKBKontur.Catalogue.WebTestCore.TestSystem;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.TestBases
{
    public class MonitoringFunctionalTestBase : WebDriverFunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            container = ContainerCache.GetContainer(ContainerCacheKey, "monitoringTestsSettings", ConfigureContainer);
            ClearAllBeforeTest(container);
            DropAndCreateDatabase(container.Get<IColumnFamilyRegistry>().GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily
                        {
                            Name = TestCassandraCounterBlobRepository.columnFamilyName,
                        }
                }).ToArray());
            container.Get<IRemoteTaskQueueMonitoringServiceClient>().DropLocalStorage();
            container.Get<IRemoteTaskQueueMonitoringServiceClient>().ActualizeDatabaseScheme();
            container.Get<IExchangeServiceClient>().Start();
        }

        private static void ClearAllBeforeTest(IContainer container)
        {
            var cassandraSchemeActualizer = container.Get<ICassandraSchemeActualizer>();
            cassandraSchemeActualizer.AddNewColumnFamilies();
            cassandraSchemeActualizer.TruncateAllColumnFamilies();
            container.Get<IExchangeServiceClient>().Stop();
            container.Get<IExchangeServiceClient>().Start();
        }

        public override void TearDown()
        {
            container.Get<IExchangeServiceClient>().Stop();
            base.TearDown();
        }

        protected TasksListPage LoadTasksListPage()
        {
            return LoadPage<TasksListPage>("/AdminTools/RemoteTaskQueue");
        }

        protected virtual void ConfigureContainer(IContainer c)
        {
            c.Get<RemoteTaskQueueMonitoringSchemaConfiguration>().ConfigureBusinessObjectStorage(c);
        }

        protected virtual string ContainerCacheKey { get { return "RemoteTaskQueue.MonitoringTests"; } }

        protected override FunctionTestsConfiguration Configuration { get { return configuration; } }

        protected IContainer container;

        private void DropAndCreateDatabase(ColumnFamily[] columnFamilies)
        {
            var cassandraCluster = container.Get<ICassandraCluster>();
            var settings = container.Get<ICassandraSettings>();
            var clusterConnection = cassandraCluster.RetrieveClusterConnection();
            var keyspaceConnection = cassandraCluster.RetrieveKeyspaceConnection(settings.QueueKeyspace);

            var keyspaces = clusterConnection.RetrieveKeyspaces();
            if(!keyspaces.Any(x => x.Name == settings.QueueKeyspace))
            {
                clusterConnection.AddKeyspace(
                    new Keyspace
                        {
                            Name = settings.QueueKeyspace,
                            ReplicaPlacementStrategy = "org.apache.cassandra.locator.SimpleStrategy",
                            ReplicationFactor = 1
                        });
            }

            var cassandraColumnFamilies = keyspaceConnection.DescribeKeyspace().ColumnFamilies;
            foreach(var columnFamily in columnFamilies)
            {
                if(!cassandraColumnFamilies.Any(x => x.Key == columnFamily.Name))
                    keyspaceConnection.AddColumnFamily(columnFamily);
            }

            foreach(var columnFamily in columnFamilies)
            {
                var columnFamilyConnection = cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, columnFamily.Name);
                columnFamilyConnection.Truncate();
            }
        }

        private static readonly FunctionTestsConfiguration configuration = new FunctionTestsConfiguration(9876);
    }
}