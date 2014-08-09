using System.Linq;

using ExchangeService.UserClasses;

using FunctionalTests.Logging;

using GroBuf;
using GroBuf.DataMembersExtracters;

using GroboContainer.Core;
using GroboContainer.Impl;

using NUnit.Framework;

using RemoteQueue.Configuration;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
using SKBKontur.Catalogue.ServiceLib;

namespace FunctionalTests
{
    [TestFixture]
    public class FunctionalTestBaseWithoutServices
    {
        [SetUp]
        public virtual void SetUp()
        {
            Container = new Container(new ContainerConfiguration(AssembliesLoader.Load()));
            var applicationSettings = ApplicationSettings.LoadDefault("functionalTestsSettings");
            Container.Configurator.ForAbstraction<IApplicationSettings>().UseInstances(applicationSettings);
            Container.Configurator.ForAbstraction<ISerializer>().UseInstances(new Serializer(new AllPropertiesExtractor(), null, GroBufOptions.MergeOnRead));
            Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(Container.Get<RemoteQueueTestsCassandraSettings>());
            Container.ConfigureLockRepository();
            Log4NetConfiguration.InitializeOnce();
            var columnFamilyRegistry = Container.Get<IColumnFamilyRegistry>();
            var columnFamilies = columnFamilyRegistry.GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily
                        {
                            Name = TestCassandraCounterBlobRepository.columnFamilyName,
                        }
                }).ToArray();

            DropAndCreateDatabase(columnFamilies);
        }

        [TearDown]
        public virtual void TearDown()
        {
        }

        protected Container Container { get; private set; }

        private void DropAndCreateDatabase(ColumnFamily[] columnFamilies)
        {
            var cassandraCluster = Container.Get<ICassandraCluster>();
            var settings = Container.Get<ICassandraSettings>();
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
    }
}