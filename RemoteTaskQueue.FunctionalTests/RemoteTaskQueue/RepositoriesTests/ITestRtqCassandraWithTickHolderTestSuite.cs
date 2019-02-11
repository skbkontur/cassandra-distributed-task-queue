using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Catalogue.TestCore.NUnit.Extensions;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    [WithDefaultSerializer, WithTestRemoteTaskQueueSettings, WithCassandra(TestRemoteTaskQueueSettings.QueueKeyspaceName), AndResetCassandraState]
    public interface ITestRtqCassandraWithTickHolderTestSuite
    {
    }
}