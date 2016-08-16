using System.Linq;

using ExchangeService.UserClasses;

using GroboContainer.Core;

using RemoteQueue.Configuration;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraStorageCore;
using SKBKontur.Catalogue.CassandraStorageCore.Initializing;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases;
using SKBKontur.Catalogue.RemoteTaskQueue.Storage;
using SKBKontur.Catalogue.TestCore;
using SKBKontur.Catalogue.WebTestCore;
using SKBKontur.Catalogue.WebTestCore.TestSystem;

using TestCommon;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.TestBases
{
    public class MonitoringFunctionalTestBase : WebDriverFunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            container = ContainerCache.GetContainer("RemoteTaskQueue.MonitoringTests", "monitoringTests.csf", ConfigureContainer);
            container.Get<IExchangeServiceClient>().Stop();
            ResetTaskQueueCassandraState();
            ResetBusinessObjectStorageState();
            ResetTaskQueueMonitoringState();
            container.Get<IExchangeServiceClient>().Start();
        }

        private void ConfigureContainer(IContainer c)
        {
            c.Configurator.ForAbstraction<ICassandraClusterSettings>().UseType<RemoteQueueTestsCassandraSettings>();
            c.ConfigureRemoteTaskQueue();
            c.Configurator.ForAbstraction<ICassandraCoreSettings>().UseType<TestCassandraCoreSettings>();
            c.Get<RemoteTaskQueueMonitoringSchemaConfiguration>().ConfigureBusinessObjectStorage(c);
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
            container.DropAndCreateDatabase(container.Get<IColumnFamilyRegistry>().GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily
                        {
                            Name = TestCounterRepository.CfName,
                        }
                }).ToArray());
        }

        protected IContainer container;

        private static readonly FunctionTestsConfiguration configuration = new FunctionTestsConfiguration(9876);
    }
}