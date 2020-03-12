using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;
using SkbKontur.Cassandra.GlobalTimestamp;

using SKBKontur.Catalogue.TestCore.NUnit.Extensions;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    [WithDefaultSerializer, WithCassandra(TestRtqSettings.QueueKeyspaceName), WithNoProfiling]
    public class WithTestRemoteTaskQueue : GroboTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableGroboTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<IGlobalTime>().UseType<GlobalTimeProxy>();
            suiteContext.Container.Configurator.ForAbstraction<IRtqTaskDataRegistry>().UseType<TestTaskDataRegistry>();
            suiteContext.Container.Configurator.ForAbstraction<IRtqSettings>().UseType<TestRtqSettings>();
            suiteContext.Container.Configurator.ForAbstraction<IRtqProfiler>().UseType<NoOpRtqProfiler>();
        }
    }
}