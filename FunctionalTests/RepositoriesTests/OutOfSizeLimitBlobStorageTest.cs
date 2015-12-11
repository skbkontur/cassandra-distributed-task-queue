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
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace FunctionalTests.RepositoriesTests
{
    public class OutOfSizeLimitBlobStorageTest : FunctionalTestBaseWithoutServices
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
            if(keyspace.ColumnFamilies.All(x => x.Key != columnFamilyName))
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
        [ExpectedException(typeof(InvalidProgramStateException))]
        public void TestExceptionOnWrite()
        {
            timeBasedBlobStorage.Write(TimeGuid.NowGuid(), new byte[TimeBasedBlobStorageSettings.BlobSizeLimit + 1]);
        }

        [Test]
        public void TestWriteDataMoreBlobSizeLimit()
        {
            string returnId;
            Assert.That(blobStorageDecorator.TryWrite(new byte[TimeBasedBlobStorageSettings.BlobSizeLimit + 1], out returnId), Is.True);
            TimeGuid timeGuid;
            Assert.That(TimeGuid.TryParse(returnId, out timeGuid), Is.False);
            Assert.That(blobStorage.Read(returnId), Is.Not.Null);
        }

        private const string blobStorageColumnFamilyName = "blobStorageTest";
        private const string timeBasedBlobStorageColumnFamilyName = "timeBasedBlobStorageTest";
        private BlobStorageDecorator<byte[]> blobStorageDecorator;
        private BlobStorage<byte[]> blobStorage;
        private TimeBasedBlobStorage<byte[]> timeBasedBlobStorage;
        private IKeyspaceConnection connection;
    }
}