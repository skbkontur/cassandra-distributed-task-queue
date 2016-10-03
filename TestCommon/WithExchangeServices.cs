using System.Reflection;

using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;

namespace TestCommon
{
    [WithDefaultSerializer]
    public class WithExchangeServices : EdiTestSuiteWrapperAttribute
    {
        public override void SetUp(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            suiteContext.Container.Get<IExchangeServiceClient>().Start();
            suiteContext.Container.Get<IExchangeServiceClient>().ChangeTaskTtl(RemoteQueueTestsCassandraSettings.StandardTestTaskTtl);
        }

        public override void TearDown(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            suiteContext.Container.Get<IExchangeServiceClient>().Stop();
        }
    }
}