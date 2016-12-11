using System.Reflection;

using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;

namespace RemoteTaskQueue.FunctionalTests
{
    [WithDefaultSerializer]
    public class WithExchangeServices : EdiTestSuiteWrapperAttribute
    {
        public override void SetUp(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            suiteContext.Container.Get<ExchangeServiceClient>().Start();
            suiteContext.Container.Get<ExchangeServiceClient>().ChangeTaskTtl(RemoteQueueTestsCassandraSettings.StandardTestTaskTtl);
        }

        public override void TearDown(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            suiteContext.Container.Get<ExchangeServiceClient>().Stop();
        }
    }
}