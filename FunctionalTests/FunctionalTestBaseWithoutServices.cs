using System.Linq;

using ExchangeService.UserClasses;

using GroboContainer.Core;
using GroboContainer.Impl;

using GroBuf;
using GroBuf.DataMembersExtracters;

using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Configuration;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
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
            container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseType<RemoteQueueTestsCassandraSettings>();
            container.ConfigureRemoteTaskQueue();
            container.ConfigureLockRepository();
        }

        [TearDown]
        public virtual void TearDown()
        {
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
            Container.Get<ElasticMonitoringServiceClient>().ResetState();
        }

        private void ResetTicksHolderState()
        {
            Container.Get<TicksHolder>().ResetInMemoryState();
        }
    }
}