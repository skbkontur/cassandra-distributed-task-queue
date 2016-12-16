using System.Reflection;

using GroboContainer.Core;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Configuration;
using RemoteQueue.Settings;

using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock.RemoteLocker;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;
using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    [WithDefaultSerializer, WithCassandra(TestRemoteTaskQueueSettings.QueueKeyspaceName), WithEmptyGraphiteClient, WithNoProfiling]
    public class WithTestRemoteTaskQueue : EdiTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            ConfigureContainer(suiteContext.Container);
        }

        public static void ConfigureContainer(IContainer container)
        {
            container.Configurator.ForAbstraction<ITaskDataRegistry>().UseType<TestTaskDataRegistry>();
            container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseType<TestRemoteTaskQueueSettings>();
            ConfigureRemoteLock(container);
        }

        private static void ConfigureRemoteLock(IContainer container)
        {
            var keyspaceName = container.Get<IRemoteTaskQueueSettings>().QueueKeyspace;
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(new ColumnFamilyFullName(keyspaceName, RemoteTaskQueueLockConstants.LockColumnFamily));
            var remoteLockImplementation = container.Create<CassandraRemoteLockImplementationSettings, CassandraRemoteLockImplementation>(remoteLockImplementationSettings);
            container.Configurator.ForAbstraction<IRemoteLockCreator>().UseInstances(new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics(keyspaceName)));
        }
    }
}