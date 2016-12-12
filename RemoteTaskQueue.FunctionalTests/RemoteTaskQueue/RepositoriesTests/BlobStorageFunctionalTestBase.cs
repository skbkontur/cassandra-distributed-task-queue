using System.Diagnostics.CodeAnalysis;

using GroBuf;

using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
    [EdiTestSuite("BlobStorageTests"), WithDefaultSerializer, WithTestRemoteTaskQueueSettings, WithCassandra(QueueKeyspaceName)]
    public abstract class BlobStorageFunctionalTestBase
    {
        protected void ResetCassandraState()
        {
            EdiTestContext.Current.Container.ResetCassandraState(QueueKeyspaceName, GetColumnFamilies());
        }

        protected abstract ColumnFamily[] GetColumnFamilies();

        protected const string QueueKeyspaceName = TestRemoteTaskQueueSettings.QueueKeyspaceName;

        [Injected]
        protected readonly ISerializer serializer;

        [Injected]
        protected readonly ICassandraCluster cassandraCluster;
    }
}