using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
{
    [AndResetExchangeServiceState]
    public class AndResetMonitoringServiceState : GroboTestMethodWrapperAttribute
    {
        public override sealed void SetUp(string testName, IEditableGroboTestContext suiteContext, IEditableGroboTestContext methodContext)
        {
            suiteContext.Container.Get<MonitoringServiceClient>().ResetState();
        }

        public override void TearDown(string testName, IEditableGroboTestContext suiteContext, IEditableGroboTestContext methodContext)
        {
            suiteContext.Container.Get<MonitoringServiceClient>().Stop();
        }
    }
}