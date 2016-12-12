using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;

namespace RemoteTaskQueue.FunctionalTests
{
    [AndResetCassandraState]
    public class AndResetExchangeServiceState : EdiTestMethodWrapperAttribute
    {
        public override sealed void SetUp(string testName, IEditableEdiTestContext suiteContext, IEditableEdiTestContext methodContext)
        {
            suiteContext.Container.Get<ExchangeServiceClient>().Start();
            suiteContext.Container.Get<ExchangeServiceClient>().ChangeTaskTtl(TestRemoteTaskQueueSettings.StandardTestTaskTtl);
        }

        public override void TearDown(string testName, IEditableEdiTestContext suiteContext, IEditableEdiTestContext methodContext)
        {
            suiteContext.Container.Get<ExchangeServiceClient>().Stop();
        }
    }
}