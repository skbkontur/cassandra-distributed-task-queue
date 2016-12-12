using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    [WithDefaultSerializer, WithTestRemoteTaskQueueSettings, WithCassandra(TestRemoteTaskQueueSettings.QueueKeyspaceName), AndResetCassandraState, AndResetTicksHolderState]
    public interface ITestRtqCassandraWithTickHolderTestSuite
    {
    }
}