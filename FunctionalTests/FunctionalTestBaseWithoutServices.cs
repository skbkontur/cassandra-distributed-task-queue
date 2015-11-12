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
using SKBKontur.Cassandra.CassandraClient.Scheme;
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
            Container.Configurator.ForAbstraction<ISerializer>().UseInstances(new Serializer(new AllPropertiesExtractor(), null, GroBufOptions.MergeOnRead));
            Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(Container.Get<RemoteQueueTestsCassandraSettings>());
            Container.ConfigureLockRepository();
            Log4NetConfiguration.InitializeOnce();
            ResetTaskQueueCassandraState();
            ResetTaskQueueMonitoringState();
        }

        private void ResetTaskQueueMonitoringState()
        {
            var client = Container.Get<IRemoteTaskQueueMonitoringServiceClient>();
            client.DropLocalStorage();
            client.ActualizeDatabaseScheme();
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

        private void ResetTaskQueueCassandraState()
        {
            var columnFamilies = Container.Get<IColumnFamilyRegistry>().GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily
                        {
                            Name = TestCassandraCounterBlobRepository.columnFamilyName,
                        },
                    new ColumnFamily
                        {
                            Name = CassandraTestTaskLogger.columnFamilyName
                        }
                }).ToArray();
            var cassandraCluster = Container.Get<ICassandraCluster>();
            var settings = Container.Get<ICassandraSettings>();
            cassandraCluster.ActualizeKeyspaces(new[]
                {
                    new KeyspaceScheme
                        {
                            Name = settings.QueueKeyspace,
                            Configuration =
                                {
                                    ReplicationFactor = 1,
                                    ReplicaPlacementStrategy = ReplicaPlacementStrategy.Simple,
                                    ColumnFamilies = columnFamilies,
                                }
                        },
                });
            foreach(var columnFamily in columnFamilies)
                cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, columnFamily.Name).Truncate();
        }
    }
}