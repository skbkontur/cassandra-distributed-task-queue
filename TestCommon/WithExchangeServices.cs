using System.Reflection;

using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;

namespace TestCommon
{
    [WithDefaultSerializer]
    public class WithExchangeServices : EdiTestSuiteWrapperAttribute
    {
        public override void SetUp(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            suiteContext.Container.Get<IExchangeServiceClient>().Start();
        }

        public override void TearDown(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            suiteContext.Container.Get<IExchangeServiceClient>().Stop();
        }
    }
}