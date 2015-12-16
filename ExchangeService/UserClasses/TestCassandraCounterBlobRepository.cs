using GroBuf;

using RemoteQueue.Cassandra.Repositories.BlobStorages;

using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace ExchangeService.UserClasses
{
    public class TestCassandraCounterBlobRepository : LegacyBlobStorage<int>
    {
        public TestCassandraCounterBlobRepository(ICassandraCluster cassandraCluster, ISerializer serializer, string keyspaceName)
            : base(cassandraCluster, serializer, keyspaceName, columnFamilyName)
        {
        }

        public const string columnFamilyName = "columnFamilyName";
    }
}