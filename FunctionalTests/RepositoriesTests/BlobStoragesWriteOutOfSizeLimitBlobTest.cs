using System.Linq;

using GroBuf;

using NUnit.Framework;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Connections;
using SKBKontur.Catalogue.Objects;

namespace FunctionalTests.RepositoriesTests
{
    public class BlobStoragesWriteOutOfSizeLimitBlobTest : FunctionalTestBaseWithoutServices
    {
        public override void SetUp()
        {
            base.SetUp();
            var repositoryParameters = Container.Get<IColumnFamilyRepositoryParameters>();
            var serializer = Container.Get<ISerializer>();
            var globalTime = Container.Get<IGlobalTime>();
            var cassandraCluster = Container.Get<ICassandraCluster>();
            var settings = Container.Get<ICassandraSettings>();

            connection = cassandraCluster.RetrieveKeyspaceConnection(settings.QueueKeyspace);
            AddColumnFamily(blobStorageColumnFamilyName);
            AddColumnFamily(timeBasedBlobStorageColumnFamilyName);

            blobStorageDecorator = new BlobStorageDecorator<byte[]>(repositoryParameters, serializer, globalTime, blobStorageColumnFamilyName, timeBasedBlobStorageColumnFamilyName);
            blobStorage = new BlobStorage<byte[]>(repositoryParameters, serializer, globalTime, blobStorageColumnFamilyName);
            timeBasedBlobStorage = new TimeBasedBlobStorage<byte[]>(repositoryParameters, serializer, globalTime, timeBasedBlobStorageColumnFamilyName);
        }

        public override void TearDown()
        {
            connection.RemoveColumnFamily(blobStorageColumnFamilyName);
            connection.RemoveColumnFamily(timeBasedBlobStorageColumnFamilyName);
            base.TearDown();
        }

        private void AddColumnFamily(string columnFamilyName)
        {
            var keyspace = connection.DescribeKeyspace();
            if(!keyspace.ColumnFamilies.Any(x => x.Key == columnFamilyName))
            {
                connection.AddColumnFamily(new ColumnFamily
                    {
                        Name = columnFamilyName,
                        GCGraceSeconds = 10,
                        Caching = ColumnFamilyCaching.All
                    });
            }
        }

        [Test]
        public void TestTimeBasedStorageNotWriteBigBlob()
        {
            var blob = new byte[TimeBasedBlobStorageSettings.BlobSizeLimit + 1];
            Assert.That(timeBasedBlobStorage.Write(timeGuidId, blob), Is.EqualTo(BlobWriteResult.OutOfSizeLimit));
            Assert.That(timeBasedBlobStorage.Read(timeGuidId), Is.Null);
        }

        [Test]
        public void TestTimeBasedBlobWriteToBlobStorage()
        {
            var blob = new byte[TimeBasedBlobStorageSettings.BlobSizeLimit + 1];
            var blobWriteResult = blobStorageDecorator.Write(timeGuidId.ToGuid().ToString(), blob);
            Assert.That(blobWriteResult, Is.EqualTo(BlobWriteResult.Success));
            Assert.That(timeBasedBlobStorage.Read(timeGuidId), Is.Null);
            Assert.That(blobStorage.Read(timeGuidId.ToGuid().ToString()).Length, Is.EqualTo(TimeBasedBlobStorageSettings.BlobSizeLimit + 1));
        }

        private const string blobStorageColumnFamilyName = "blobStorageTest";
        private const string timeBasedBlobStorageColumnFamilyName = "orderedBlobStorageTest";
        private BlobStorageDecorator<byte[]> blobStorageDecorator;
        private BlobStorage<byte[]> blobStorage;
        private TimeBasedBlobStorage<byte[]> timeBasedBlobStorage;
        private readonly TimeGuid timeGuidId = TimeGuid.NowGuid();
        private IKeyspaceConnection connection;
    }
}