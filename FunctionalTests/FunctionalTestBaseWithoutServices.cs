using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using ExchangeService.UserClasses;

using GroboContainer.Core;
using GroboContainer.Impl;

using GroBuf;
using GroBuf.DataMembersExtracters;

using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Configuration;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.TestCore;

using TestCommon;

namespace FunctionalTests
{
    [TestFixture]
    public abstract class FunctionalTestBaseWithoutServices
    {
        [SetUp]
        public virtual void SetUp()
        {
            Log4NetHelper.SetUpLoggingOnce("RemoteTaskQueue");
            Container = new Container(new ContainerConfiguration(AssembliesLoader.Load()));
            ConfigureContainer(Container);
            ResetTaskQueueCassandraState();
            ResetTaskQueueMonitoringState();
            ResetTicksHolderState();
        }

        protected virtual void ConfigureContainer(Container container)
        {
            container.Configurator.ForAbstraction<ISerializer>().UseInstances(new Serializer(new AllPropertiesExtractor(), null, GroBufOptions.MergeOnRead));
			container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(Container.Get<RemoteQueueTestsCassandraSettings>());
            container.ConfigureRemoteTaskQueue();
            container.ConfigureLockRepository();
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
                            Name = TestCounterRepository.CfName,
                        },
                    new ColumnFamily
                        {
                            Name = CassandraTestTaskLogger.columnFamilyName
                        }
                }).ToArray();
            Container.DropAndCreateDatabase(columnFamilies);
        }

        private void ResetTaskQueueMonitoringState()
        {
            var client = Container.Get<IRemoteTaskQueueMonitoringServiceClient>();
            client.DropLocalStorage();
            client.ActualizeDatabaseScheme();
        }

        private void ResetTicksHolderState()
        {
            Container.Get<TicksHolder>().ResetInMemoryState();
        }
    }
}