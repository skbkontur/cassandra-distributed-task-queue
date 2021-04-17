using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.GlobalTimestamp;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
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