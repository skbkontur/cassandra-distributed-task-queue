using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;
using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.FunctionalTests
{
    [AndResetExchangeServiceState]
    public class AndResetTaskCounterServiceState : EdiTestMethodWrapperAttribute
    {
        public override sealed void SetUp(string testName, IEditableEdiTestContext suiteContext, IEditableEdiTestContext methodContext)
        {
            suiteContext.Container.Get<TaskCounterServiceClient>().Start();
            suiteContext.Container.Get<TaskCounterServiceClient>().RestartProcessingTaskCounter(Timestamp.Now.ToDateTime());
        }

        public override void TearDown(string testName, IEditableEdiTestContext suiteContext, IEditableEdiTestContext methodContext)
        {
            suiteContext.Container.Get<TaskCounterServiceClient>().Stop();
        }
    }
}