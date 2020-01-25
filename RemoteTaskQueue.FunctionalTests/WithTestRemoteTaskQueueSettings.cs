using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Settings;

using RemoteTaskQueue.FunctionalTests.Common;

using SkbKontur.Cassandra.GlobalTimestamp;

namespace RemoteTaskQueue.FunctionalTests
{
    public class WithTestRemoteTaskQueueSettings : GroboTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableGroboTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<IGlobalTime>().UseType<RtqGlobalTimeProxy>();
            suiteContext.Container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseType<TestRemoteTaskQueueSettings>();
        }
    }
}