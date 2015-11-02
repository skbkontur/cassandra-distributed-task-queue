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
using SKBKontur.Catalogue.Objects;

namespace FunctionalTests.RepositoriesTests
{
    public class OrderedBlobStorageTest : FunctionalTestBaseWithoutServices
    {
        public override void SetUp()
        {
            base.SetUp();
            var repositoryParameters = Container.Get<IColumnFamilyRepositoryParameters>();
            var serializer = Container.Get<ISerializer>();
            var globalTime = Container.Get<IGlobalTime>();
            var cassandraCluster = Container.Get<ICassandraCluster>();
            var settings = Container.Get<ICassandraSettings>();
            const string columnFamilyName = "OrderedBlobStorageTest";
            var connection = cassandraCluster.RetrieveKeyspaceConnection(settings.QueueKeyspace);
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
            orderedBlobStorage = new OrderedBlobStorage<int?>(repositoryParameters, serializer, globalTime, columnFamilyName);
        }

        [Test]
        public void TestInvalidMethodCallsWithoutTimeGuid()
        {
            const string id = "some-string";
            Assert.Throws<ArgumentException>(() => orderedBlobStorage.Write(id, 1));
            Assert.Throws<ArgumentException>(() => orderedBlobStorage.Read(id));
            Assert.Throws<ArgumentException>(() => orderedBlobStorage.Write(new[] { new KeyValuePair<string, int?>(id, 1)}));
            Assert.Throws<ArgumentException>(() => orderedBlobStorage.ReadQuiet(new[] { id }));
            Assert.Throws<ArgumentException>(() => orderedBlobStorage.Delete(id, 100));
            Assert.Throws<ArgumentException>(() => orderedBlobStorage.Delete(new []{id}, 100));
        }

        [Test]
        public void TestReadWrite()
        {
            var id = TimeGuid.NowGuid().ToGuid().ToString();
            Assert.IsNull(orderedBlobStorage.Read(id));

            orderedBlobStorage.Write(id, 10);
            Assert.That(orderedBlobStorage.Read(id), Is.EqualTo(10));

            orderedBlobStorage.Write(id, 11);
            Assert.That(orderedBlobStorage.Read(id), Is.EqualTo(11));
        }

        [Test]
        public void TestMultiWrite()
        {
            var id1 = TimeGuid.NowGuid().ToGuid().ToString();
            var id2 = TimeGuid.NowGuid().ToGuid().ToString();

            orderedBlobStorage.Write(new []{new KeyValuePair<string, int?>(id1, 1), new KeyValuePair<string, int?>(id2, 2)});
            Assert.That(orderedBlobStorage.Read(id1), Is.EqualTo(1));
            Assert.That(orderedBlobStorage.Read(id2), Is.EqualTo(2));
        }
        
        [Test]
        public void TestReadQuiet()
        {
            var id1 = TimeGuid.NowGuid().ToGuid().ToString();
            var id2 = TimeGuid.NowGuid().ToGuid().ToString();

            Assert.That(orderedBlobStorage.ReadQuiet(new []{id1, id2}), Is.EquivalentTo(new int?[]{null, null}));

            orderedBlobStorage.Write(id1, 1);
            Assert.That(orderedBlobStorage.ReadQuiet(new[] { id1, id2 }), Is.EqualTo(new int?[] { 1, null }));
            Assert.That(orderedBlobStorage.ReadQuiet(new[] { id2, id1 }), Is.EqualTo(new int?[] { null, 1 }));

            orderedBlobStorage.Write(id2, 2);
            Assert.That(orderedBlobStorage.ReadQuiet(new[] { id1, id2 }), Is.EqualTo(new int?[] { 1, 2 }));
            Assert.That(orderedBlobStorage.ReadQuiet(new[] { id2, id1 }), Is.EqualTo(new int?[] { 2, 1 }));
        }

        [Test]
        public void TestMultiRead()
        {
            var id1 = TimeGuid.NowGuid().ToGuid().ToString();
            var id2 = TimeGuid.NowGuid().ToGuid().ToString();

            Assert.That(orderedBlobStorage.Read(new []{id1, id2}).Length, Is.EqualTo(0));

            orderedBlobStorage.Write(id1, 1);
            Assert.That(orderedBlobStorage.Read(new[] { id1, id2 }), Is.EqualTo(new int?[]{1}));

            orderedBlobStorage.Write(id2, 2);
            Assert.That(orderedBlobStorage.Read(new[] {id1, id2}), Is.EqualTo(new int?[] { 1, 2 }));
        }

        [Test]
        public void TestDelete()
        {
            var id = TimeGuid.NowGuid().ToGuid().ToString();

            orderedBlobStorage.Write(id, 1);
            orderedBlobStorage.Delete(id, DateTime.UtcNow.Ticks);

            Assert.IsNull(orderedBlobStorage.Read(id));
        }

        [Test]
        public void TestMultiDelete()
        {
            var id1 = TimeGuid.NowGuid().ToGuid().ToString();
            var id2 = TimeGuid.NowGuid().ToGuid().ToString();

            orderedBlobStorage.Write(id1, 1);
            orderedBlobStorage.Write(id2, 2);

            orderedBlobStorage.Delete(new []{id1, id2}, DateTime.UtcNow.Ticks);
            Assert.That(orderedBlobStorage.Read(new []{id1, id2}).Length, Is.EqualTo(0));
        }

        private OrderedBlobStorage<int?> orderedBlobStorage;
    }
}