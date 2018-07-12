using System.Reflection;

using RemoteQueue.Settings;

using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;

namespace RemoteTaskQueue.FunctionalTests
{
    public class WithTestRemoteTaskQueueSettings : EdiTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseType<TestRemoteTaskQueueSettings>();
        }
    }
}