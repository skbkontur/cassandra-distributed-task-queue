using System.Diagnostics.CodeAnalysis;

using GroboContainer.NUnitExtensions;

using RemoteTaskQueue.FunctionalTests.Common;

using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.ThriftClient.Clusters;

using SKBKontur.Catalogue.TestCore.NUnit.Extensions;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
    [GroboTestSuite("RepositoryTests"), WithDefaultSerializer, WithTestRtqSettings, WithCassandra(QueueKeyspaceName)]
    public abstract class RepositoryFunctionalTestBase
    {
        protected void ResetCassandraState()
        {
            GroboTestContext.Current.Container.ResetCassandraState(QueueKeyspaceName, GetColumnFamilies());
        }

        protected abstract ColumnFamily[] GetColumnFamilies();

        protected const string QueueKeyspaceName = TestRtqSettings.QueueKeyspaceName;

        [Injected]
        protected readonly ICassandraCluster cassandraCluster;
    }
}