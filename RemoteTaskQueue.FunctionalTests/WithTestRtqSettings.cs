using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using RemoteTaskQueue.FunctionalTests.Common;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.GlobalTimestamp;

namespace RemoteTaskQueue.FunctionalTests
{
    public class WithTestRtqSettings : GroboTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableGroboTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<IGlobalTime>().UseType<GlobalTimeProxy>();
            suiteContext.Container.Configurator.ForAbstraction<IRtqSettings>().UseType<TestRtqSettings>();
        }
    }
}