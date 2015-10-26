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
            container = ContainerCache.GetContainer("RemoteTaskQueue.MonitoringTests", "monitoringTests.csf",
                                                    c => c.Get<RemoteTaskQueueMonitoringSchemaConfiguration>().ConfigureBusinessObjectStorage(c));
            container.Get<IExchangeServiceClient>().Stop();
            ResetTaskQueueCassandraState();
            ResetBusinessObjectStorageState();
            ResetTaskQueueMonitoringState();
            container.Get<IExchangeServiceClient>().Start();
        }

        private void ResetBusinessObjectStorageState()
        {
            var cassandraSchemeActualizer = container.Get<ICassandraSchemeActualizer>();
            cassandraSchemeActualizer.AddNewColumnFamilies();
            cassandraSchemeActualizer.TruncateAllColumnFamilies();
        }

        private void ResetTaskQueueMonitoringState()
        {
            var client = container.Get<IRemoteTaskQueueMonitoringServiceClient>();
            client.DropLocalStorage();
            client.ActualizeDatabaseScheme();
        }

        protected TasksListPage LoadTasksListPage()
        {
            return LoadPage<TasksListPage>("/AdminTools/RemoteTaskQueue");
        }

        protected override FunctionTestsConfiguration Configuration { get { return configuration; } }

        private void ResetTaskQueueCassandraState()
        {
            var columnFamilies = container.Get<IColumnFamilyRegistry>().GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily
                        {
                            Name = TestCassandraCounterBlobRepository.columnFamilyName,
                        }
                }).ToArray();

            var cassandraCluster = container.Get<ICassandraCluster>();
            var settings = container.Get<ICassandraSettings>();
            var clusterConnection = cassandraCluster.RetrieveClusterConnection();
            var keyspaceConnection = cassandraCluster.RetrieveKeyspaceConnection(settings.QueueKeyspace);

            var keyspaces = clusterConnection.RetrieveKeyspaces();
            if(keyspaces.All(x => x.Name != settings.QueueKeyspace))
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
                if(cassandraColumnFamilies.All(x => x.Key != columnFamily.Name))
                    keyspaceConnection.AddColumnFamily(columnFamily);
            }

            foreach(var columnFamily in columnFamilies)
            {
                var columnFamilyConnection = cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, columnFamily.Name);
                columnFamilyConnection.Truncate();
            }
        }

        protected IContainer container;

        private static readonly FunctionTestsConfiguration configuration = new FunctionTestsConfiguration(9876);
    }
}