using System;
using System.Linq;

using ExchangeService.UserClasses;

using GroboContainer.Core;
using GroboContainer.Impl;

using RemoteQueue.Configuration;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.AccessControl;
using SKBKontur.Catalogue.AccessControl.AccessRules;
using SKBKontur.Catalogue.AccessControl.Services;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases;
using SKBKontur.Catalogue.RemoteTaskQueue.Storage;
using SKBKontur.Catalogue.ServiceLib;
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
            container.ClearAllBeforeTest();
            DropAndCreateDatabase(container.Get<IColumnFamilyRegistry>().GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily
                        {
                            Name = TestCassandraCounterBlobRepository.columnFamilyName,
                        }
                }).ToArray());
            userRepository = container.Get<IUserRepository>();
            accessControlService = container.Get<IAccessControlService>();
            passwordService = container.Get<IPasswordService>();
            DefaultPage = LoadPage<DefaultPage>("");
            container.Get<IRemoteTaskQueueMonitoringServiceClient>().DropLocalStorage();
            container.Get<IRemoteTaskQueueMonitoringServiceClient>().ActualizeDatabaseScheme();
            container.Get<IExchangeServiceClient>().Start();
        }

        public override void TearDown()
        {
            container.Get<IExchangeServiceClient>().Stop();
            base.TearDown();
        }

        protected void CreateUser(string login, string password)
        {
            userRepository.ReleaseLogin(login);
            var userId = Guid.NewGuid().ToString();
            userRepository.SaveUser(new User
            {
                Id = userId,
                PasswordHash = passwordService.GetPasswordHash(password),
                Login = login,
                UserName = login
            });
            accessControlService.AddAccess(userId, new ResourseGroupAccessRule
            {
                ResourseGroupName = ResourseGroups.AdminResourse
            });
        }

        private void DropAndCreateDatabase(ColumnFamily[] columnFamilies)
        {
            var cassandraCluster = container.Get<ICassandraCluster>();
            var settings = container.Get<ICassandraSettings>();
            var clusterConnection = cassandraCluster.RetrieveClusterConnection();
            var keyspaceConnection = cassandraCluster.RetrieveKeyspaceConnection(settings.QueueKeyspace);

            var keyspaces = clusterConnection.RetrieveKeyspaces();
            if (!keyspaces.Any(x => x.Name == settings.QueueKeyspace))
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
            foreach (var columnFamily in columnFamilies)
            {
                if (!cassandraColumnFamilies.Any(x => x.Key == columnFamily.Name))
                    keyspaceConnection.AddColumnFamily(columnFamily);
            }

            foreach (var columnFamily in columnFamilies)
            {
                var columnFamilyConnection = cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, columnFamily.Name);
                columnFamilyConnection.Truncate();
            }
        }

        protected TasksListPage Login(string login, string password)
        {
            var enterPage = DefaultPage.Enter();
            return enterPage.Login(login, password);
        }

        protected virtual void ConfigureContainer(IContainer c)
        {
            //c.Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(container.Get<CassandraSettings>());
            c.Get<RemoteTaskQueueMonitoringSchemaConfiguration>().ConfigureBusinessObjectStorage(c);
        }


        protected virtual string ContainerCacheKey { get { return "RemoteTaskQueue.MonitoringTests"; } }

        protected DefaultPage DefaultPage { get; private set; }

        protected override FunctionTestsConfiguration Configuration { get { return configuration; } }
        protected IUserRepository userRepository;

        protected IContainer container;
        private static readonly FunctionTestsConfiguration configuration = new FunctionTestsConfiguration(9876);
        private IAccessControlService accessControlService;
        private IPasswordService passwordService;
    }
}