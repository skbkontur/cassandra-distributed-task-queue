using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using RemoteTaskQueue.FunctionalTests.Common;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
{
    [AndResetCassandraState]
    public class AndResetExchangeServiceState : GroboTestMethodWrapperAttribute
    {
        public override sealed void SetUp(string testName, IEditableGroboTestContext suiteContext, IEditableGroboTestContext methodContext)
        {
            var remoteTaskQueue = suiteContext.Container.Get<SkbKontur.Cassandra.DistributedTaskQueue.Handling.RemoteTaskQueue>();
            ((IRtqInternals)remoteTaskQueue).ResetTicksHolderInMemoryState();

            suiteContext.Container.Get<ExchangeServiceClient>().Start();
            suiteContext.Container.Get<ExchangeServiceClient>().ChangeTaskTtl(TestRtqSettings.StandardTestTaskTtl);
        }

        public override void TearDown(string testName, IEditableGroboTestContext suiteContext, IEditableGroboTestContext methodContext)
        {
            suiteContext.Container.Get<ExchangeServiceClient>().Stop();
        }
    }
}