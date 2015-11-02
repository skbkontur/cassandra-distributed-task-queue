using System;
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
    public class BlobStorageDecoratorTest : FunctionalTestBaseWithoutServices
    {
        private const string blobStorageColumnFamilyName = "blobStorageTest";
        private const string orderedBlobStorageColumnFamilyName = "orderedBlobStorageTest";

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
            AddColumnFamily(orderedBlobStorageColumnFamilyName);

            blobStorageDecorator = new BlobStorageDecorator<int?>(repositoryParameters, serializer, globalTime, blobStorageColumnFamilyName, orderedBlobStorageColumnFamilyName);
            blobStorage = new BlobStorage<int?>(repositoryParameters, serializer, globalTime, blobStorageColumnFamilyName);
            orderedBlobStorage = new OrderedBlobStorage<int?>(repositoryParameters, serializer, globalTime, orderedBlobStorageColumnFamilyName);
        }

        public override void TearDown()
        {
            connection.RemoveColumnFamily(blobStorageColumnFamilyName);
            connection.RemoveColumnFamily(orderedBlobStorageColumnFamilyName);
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
        public void TestRead()
        {
            blobStorage.Write(stringId, 1);
            Assert.That(blobStorageDecorator.Read(stringId), Is.EqualTo(1));

            orderedBlobStorage.Write(timeGuidId, 2);
            Assert.That(blobStorageDecorator.Read(stringId), Is.EqualTo(1));
            Assert.That(blobStorageDecorator.Read(timeGuidId), Is.EqualTo(2));
        }

        [Test]
        public void TestWrite()
        {
            blobStorageDecorator.Write(stringId, 1);
            Assert.That(blobStorage.Read(stringId), Is.EqualTo(1));

            blobStorageDecorator.Write(timeGuidId, 2);
            Assert.That(orderedBlobStorage.Read(timeGuidId), Is.EqualTo(2));
        }

        [Test]
        public void TestMultiRead()
        {
            blobStorage.Write(stringId, 1);
            Assert.That(blobStorageDecorator.Read(new[] { stringId, timeGuidId }), Is.EqualTo(new int?[] { 1 }));

            orderedBlobStorage.Write(timeGuidId, 2);
            Assert.That(blobStorageDecorator.Read(new[] {stringId, timeGuidId}), Is.EquivalentTo(new int?[] {1, 2}));
        }

        [Test]
        public void TestReadQuiet()
        {
            blobStorage.Write(stringId, 1);
            Assert.That(blobStorageDecorator.ReadQuiet(new[] { stringId, timeGuidId }), Is.EqualTo(new int?[] { 1, null }));

            orderedBlobStorage.Write(timeGuidId, 2);
            Assert.That(blobStorageDecorator.ReadQuiet(new[] { stringId, timeGuidId }), Is.EqualTo(new int?[] { 1, 2 }));
            Assert.That(blobStorageDecorator.ReadQuiet(new[] { timeGuidId, stringId }), Is.EqualTo(new int?[] { 2, 1 }));
        }

        [Test]
        public void TestDelete()
        {
            blobStorage.Write(stringId, 1);
            blobStorageDecorator.Delete(stringId, DateTime.UtcNow.Ticks);
            Assert.IsNull(blobStorage.Read(stringId));

            orderedBlobStorage.Write(timeGuidId, 2);
            blobStorageDecorator.Delete(timeGuidId, DateTime.UtcNow.Ticks);
            Assert.IsNull(orderedBlobStorage.Read(timeGuidId));
        }

        [Test]
        public void TestMultiDelete()
        {
            blobStorage.Write(stringId, 1);
            orderedBlobStorage.Write(timeGuidId, 2);
            blobStorageDecorator.Delete(new []{stringId, timeGuidId}, DateTime.UtcNow.Ticks);

            Assert.IsNull(blobStorage.Read(stringId));
            Assert.IsNull(orderedBlobStorage.Read(timeGuidId));
        }

        [Test]
        public void TestReadAll()
        {
            blobStorage.Write(stringId, 11);
            orderedBlobStorage.Write(timeGuidId, 12);

            var anotherTimeGuidId = TimeGuid.NewGuid(new Timestamp(DateTime.UtcNow.AddDays(1))).ToGuid().ToString();

            blobStorage.Write(anotherStringId, 21);
            orderedBlobStorage.Write(anotherTimeGuidId, 22);

            Assert.That(blobStorageDecorator.ReadAll(1).ToArray(), Is.EquivalentTo(new []{11, 12, 21, 22}));
            Assert.That(blobStorageDecorator.ReadAllWithIds(1).ToArray(), Is.EquivalentTo(new[]
                {
                    new KeyValuePair<string, int?>(stringId, 11),
                    new KeyValuePair<string, int?>(timeGuidId, 12),
                    new KeyValuePair<string, int?>(anotherStringId, 21),
                    new KeyValuePair<string, int?>(anotherTimeGuidId, 22),
                }));
        }

        private const string stringId = "stringId";
        private const string anotherStringId = "anotherStringId";
        private BlobStorageDecorator<int?> blobStorageDecorator;
        private BlobStorage<int?> blobStorage;
        private OrderedBlobStorage<int?> orderedBlobStorage;
        private readonly string timeGuidId = TimeGuid.NowGuid().ToGuid().ToString();
        private IKeyspaceConnection connection;
    }
}