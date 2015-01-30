using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using ExchangeService.UserClasses;

using FunctionalTests.Logging;

using GroboContainer.Core;
using GroboContainer.Impl;

using GroBuf;
using GroBuf.DataMembersExtracters;

using NUnit.Framework;

using RemoteQueue.Configuration;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.ServiceLib;

namespace FunctionalTests
{
    [TestFixture]
    public abstract class FunctionalTestBaseWithoutServices
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
            var client = Container.Get<IRemoteTaskQueueMonitoringServiceClient>();
            client.DropLocalStorage();
            client.ActualizeDatabaseScheme();
            DropAndCreateDatabase(columnFamilies);
        }

        [TearDown]
        public virtual void TearDown()
        {
        }

        protected void WaitFor(Func<bool> func, TimeSpan timeout, int checkTimeout = 99)
        {
            var stopwatch = Stopwatch.StartNew();
            while(stopwatch.Elapsed < timeout)
            {
                Thread.Sleep(checkTimeout);
                if(func())
                    return;
            }
            Assert.Fail("Условия ожидания не выполнены за {0}", timeout);
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