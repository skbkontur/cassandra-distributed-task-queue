using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using RemoteTaskQueue.FunctionalTests.Common;

namespace RemoteTaskQueue.FunctionalTests
{
    [AndResetCassandraState]
    public class AndResetExchangeServiceState : GroboTestMethodWrapperAttribute
    {
        public override sealed void SetUp(string testName, IEditableGroboTestContext suiteContext, IEditableGroboTestContext methodContext)
        {
            suiteContext.Container.Get<RemoteQueue.Handling.RemoteTaskQueue>().ResetTicksHolderInMemoryState();
            suiteContext.Container.Get<ExchangeServiceClient>().Start();
            suiteContext.Container.Get<ExchangeServiceClient>().ChangeTaskTtl(TestRemoteTaskQueueSettings.StandardTestTaskTtl);
        }

        public override void TearDown(string testName, IEditableGroboTestContext suiteContext, IEditableGroboTestContext methodContext)
        {
            suiteContext.Container.Get<ExchangeServiceClient>().Stop();
        }
    }
}