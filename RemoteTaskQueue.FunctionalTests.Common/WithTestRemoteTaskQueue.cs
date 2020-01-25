using System.Reflection;

using GroboContainer.Core;
using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Configuration;
using RemoteQueue.Profiling;
using RemoteQueue.Settings;

using SkbKontur.Cassandra.DistributedLock;
using SkbKontur.Cassandra.DistributedLock.RemoteLocker;
using SkbKontur.Cassandra.GlobalTimestamp;

using SKBKontur.Catalogue.ServiceLib.Logging;
using SKBKontur.Catalogue.TestCore.NUnit.Extensions;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    [WithDefaultSerializer, WithCassandra(TestRemoteTaskQueueSettings.QueueKeyspaceName), WithNoProfiling]
    public class WithTestRemoteTaskQueue : GroboTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableGroboTestContext suiteContext)
        {
            ConfigureContainer(suiteContext.Container);
        }

        public static void ConfigureContainer(IContainer container)
        {
            container.Configurator.ForAbstraction<IGlobalTime>().UseType<RtqGlobalTimeProxy>();
            container.Configurator.ForAbstraction<ITaskDataRegistry>().UseType<TestTaskDataRegistry>();
            container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseType<TestRemoteTaskQueueSettings>();
            container.Configurator.ForAbstraction<IRemoteTaskQueueProfiler>().UseType<NoOpRemoteTaskQueueProfiler>();
            ConfigureRemoteLock(container);
        }

        private static void ConfigureRemoteLock(IContainer container)
        {
            var keyspaceName = container.Get<IRemoteTaskQueueSettings>().QueueKeyspace;
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(keyspaceName, RemoteTaskQueueLockConstants.LockColumnFamily);
            var remoteLockImplementation = container.Create<CassandraRemoteLockImplementationSettings, CassandraRemoteLockImplementation>(remoteLockImplementationSettings);
            container.Configurator.ForAbstraction<IRemoteLockCreator>().UseInstances(new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics(keyspaceName), Log.DefaultLogger));
        }
    }
}