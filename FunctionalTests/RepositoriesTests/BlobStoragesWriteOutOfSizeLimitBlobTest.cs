using System.Collections.Generic;
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
        public void TestTimeBasedStorageNotWriteBigBlobInMultiWrite()
        {
            var item1 = new KeyValuePair<TimeGuid, byte[]>(timeGuidId, new byte[TimeBasedBlobStorageSettings.BlobSizeLimit + 1]);
            var item2 = new KeyValuePair<TimeGuid, byte[]>(anotherTimeGuidId, new byte[100]);

            var blobsWriteResult = timeBasedBlobStorage.Write(new[] {item2, item1});
            Assert.That(blobsWriteResult.IsSuccess, Is.False);
            Assert.That(blobsWriteResult.OutOfSizeLimitBlobIndexes.Count, Is.EqualTo(1));
            Assert.That(blobsWriteResult.OutOfSizeLimitBlobIndexes.ToArray()[0], Is.EqualTo(1));
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

        [Test]
        public void TestTimeBasedBlobWriteToBlobStorageInMultiWrite()
        {
            var item1 = new KeyValuePair<string, byte[]>(timeGuidId.ToGuid().ToString(), new byte[TimeBasedBlobStorageSettings.BlobSizeLimit + 1]);
            var item2 = new KeyValuePair<string, byte[]>(anotherTimeGuidId.ToGuid().ToString(), new byte[100]);
            var item3 = new KeyValuePair<string, byte[]>(id, new byte[100]);

            var blobsWriteResult = blobStorageDecorator.Write(new[] {item3, item1, item2});
            Assert.That(blobsWriteResult.IsSuccess, Is.True);
            Assert.That(timeBasedBlobStorage.Read(timeGuidId), Is.Null);
            Assert.That(timeBasedBlobStorage.Read(anotherTimeGuidId).Length, Is.EqualTo(100));
            Assert.That(blobStorage.Read(timeGuidId.ToGuid().ToString()).Length, Is.EqualTo(TimeBasedBlobStorageSettings.BlobSizeLimit + 1));
            Assert.That(blobStorage.Read(id).Length, Is.EqualTo(100));
        }

        private const string blobStorageColumnFamilyName = "blobStorageTest";
        private const string timeBasedBlobStorageColumnFamilyName = "orderedBlobStorageTest";
        private const string id = "3D32198F-BF5D-438F-8CBB-B56CD54B0F50";
        private BlobStorageDecorator<byte[]> blobStorageDecorator;
        private BlobStorage<byte[]> blobStorage;
        private TimeBasedBlobStorage<byte[]> timeBasedBlobStorage;
        private readonly TimeGuid timeGuidId = TimeGuid.NowGuid();
        private readonly TimeGuid anotherTimeGuidId = TimeGuid.NowGuid();
        private IKeyspaceConnection connection;
    }
}