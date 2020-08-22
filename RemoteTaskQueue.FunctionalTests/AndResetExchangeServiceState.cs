using System.Reflection;

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
            var remoteTaskQueue = suiteContext.Container.Get<SkbKontur.Cassandra.DistributedTaskQueue.Handling.RemoteTaskQueue>();
            resetTicksMethod.Invoke(remoteTaskQueue, new object[0]);

            suiteContext.Container.Get<ExchangeServiceClient>().Start();
            suiteContext.Container.Get<ExchangeServiceClient>().ChangeTaskTtl(TestRtqSettings.StandardTestTaskTtl);
        }

        public override void TearDown(string testName, IEditableGroboTestContext suiteContext, IEditableGroboTestContext methodContext)
        {
            suiteContext.Container.Get<ExchangeServiceClient>().Stop();
        }

        private const string resetTicksMethodName = "SkbKontur.Cassandra.DistributedTaskQueue.Handling.IRtqInternals.ResetTicksHolderInMemoryState";

        private static readonly MethodInfo resetTicksMethod = typeof(SkbKontur.Cassandra.DistributedTaskQueue.Handling.RemoteTaskQueue).GetMethod(resetTicksMethodName, BindingFlags.Instance | BindingFlags.NonPublic);
    }
}