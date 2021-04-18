using System.Diagnostics.CodeAnalysis;

using GroboContainer.NUnitExtensions;

using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.ThriftClient.Clusters;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests.RemoteTaskQueue.RepositoriesTests
{
    [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
    [GroboTestSuite("RepositoryTests"), WithDefaultSerializer, WithTestRtqSettings]
    public abstract class RepositoryFunctionalTestBase
    {
        protected void ResetCassandraState()
        {
            GroboTestContext.Current.Container.ResetCassandraState(GetColumnFamilies());
        }

        protected abstract ColumnFamily[] GetColumnFamilies();

        [Injected]
        protected readonly ICassandraCluster cassandraCluster;
    }
}