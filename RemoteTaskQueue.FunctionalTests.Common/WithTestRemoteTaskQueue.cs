using System.Reflection;

using GroboContainer.Core;
using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;
using SkbKontur.Cassandra.DistributedTaskQueue.Settings;
using SkbKontur.Cassandra.GlobalTimestamp;

using SKBKontur.Catalogue.TestCore.NUnit.Extensions;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    [WithDefaultSerializer, WithCassandra(TestRtqSettings.QueueKeyspaceName), WithNoProfiling]
    public class WithTestRemoteTaskQueue : GroboTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableGroboTestContext suiteContext)
        {
            ConfigureContainer(suiteContext.Container);
        }

        public static void ConfigureContainer(IContainer container)
        {
            container.Configurator.ForAbstraction<IGlobalTime>().UseType<GlobalTimeProxy>();
            container.Configurator.ForAbstraction<IRtqTaskDataRegistry>().UseType<TestTaskDataRegistry>();
            container.Configurator.ForAbstraction<IRtqSettings>().UseType<TestRtqSettings>();
            container.Configurator.ForAbstraction<IRtqProfiler>().UseType<NoOpRtqProfiler>();
        }
    }
}