using System;
using System.Linq;

using GroboContainer.Core;

using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.AccessControl;
using SKBKontur.Catalogue.AccessControl.AccessRules;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases;
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
            DropAndCreateDatabase();
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
            container.Get<IExchangeServiceClient>().Start();
            try
            {
                container.Dispose();
            }
            finally
            {
                base.TearDown();
            }
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

        private void DropAndCreateDatabase()
        {
            var cassandraCluster = container.Get<ICassandraCluster>();
            var settings = container.Get<ICassandraSettings>();
            var clusterConnection = cassandraCluster.RetrieveClusterConnection();
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
        }

        protected TasksListPage Login(string login, string password)
        {
            var enterPage = DefaultPage.Enter();
            return enterPage.Login(login, password);
        }

        protected virtual void ConfigureContainer(IContainer c)
        {
        }


        protected virtual string ContainerCacheKey { get { return "RemoteTaskQueue.MonitoringTests"; } }

        protected DefaultPage DefaultPage { get; private set; }

        protected override FunctionTestsConfiguration Configuration { get { return configuration; } }
        protected IUserRepository userRepository;

        protected IContainer container;
        private static readonly FunctionTestsConfiguration configuration = new FunctionTestsConfiguration(9876, 6669);
        private IAccessControlService accessControlService;
        private IPasswordService passwordService;
    }
}