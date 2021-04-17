using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using JetBrains.Annotations;

using RemoteTaskQueue.FunctionalTests.Common;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.GlobalTimestamp;

namespace RemoteTaskQueue.FunctionalTests
{
    [WithCassandra]
    public class WithTestRtqSettings : GroboTestSuiteWrapperAttribute
    {
        public override sealed void SetUp([NotNull] string suiteName, [NotNull] Assembly testAssembly, [NotNull] IEditableGroboTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<IGlobalTime>().UseType<GlobalTimeProxy>();
            suiteContext.Container.Configurator.ForAbstraction<IRtqSettings>().UseType<TestRtqSettings>();
        }
    }
}