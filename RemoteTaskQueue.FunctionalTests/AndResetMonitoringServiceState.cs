using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;

namespace RemoteTaskQueue.FunctionalTests
{
    public class AndResetMonitoringServiceState : EdiTestMethodWrapperAttribute
    {
        public override sealed void SetUp(string testName, IEditableEdiTestContext suiteContext, IEditableEdiTestContext methodContext)
        {
            suiteContext.Container.Get<MonitoringServiceClient>().ResetState();
        }
    }
}