using System;
using System.Collections.Generic;

using GroBuf;

using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    public class LegacyBlobStorageTest : BlobStorageFunctionalTestBase
    {
        [SetUp]
        public void SetUp()
        {
            var serializer = Container.Get<ISerializer>();
            var cassandraCluster = Container.Get<ICassandraCluster>();
            var keyspaceName = Container.Get<IRemoteTaskQueueSettings>().QueueKeyspace;
            blobStorage = new LegacyBlobStorage<Dto>(cassandraCluster, serializer, keyspaceName, cfName);
        }

        protected override ColumnFamily[] GetColumnFamilies()
        {
            return new[] {new ColumnFamily {Name = cfName}};
        }

        [Test]
        public void TestReadWrite()
        {
            var id = Guid.NewGuid().ToString();
            const string field1 = "yyy";
            blobStorage.Write(id, new Dto { Field1 = field1 }, Timestamp.Now.Ticks, defaultTtl);
            var elem = blobStorage.Read(id);
            Assert.AreEqual(field1, elem.Field1);
        }

        [Test]
        public void TestMultiRead()
        {
            var id1 = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            Assert.That(blobStorage.Read(new List<string>()).Count, Is.EqualTo(0));
            Assert.That(blobStorage.Read(new List<string> {id1, id2}).Count, Is.EqualTo(0));

            blobStorage.Write(id1, new Dto { Field1 = "id1" }, Timestamp.Now.Ticks, defaultTtl);
            var actual = blobStorage.Read(new List<string> {id1, id2});
            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual[id1].Field1, Is.EqualTo("id1"));

            blobStorage.Write(id2, new Dto { Field1 = "id2" }, Timestamp.Now.Ticks, defaultTtl);
            actual = blobStorage.Read(new List<string> {id1, id2});
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual[id1].Field1, Is.EqualTo("id1"));
            Assert.That(actual[id2].Field1, Is.EqualTo("id2"));
        }

        [Test]
        public void TestTtl()
        {
            var id = Guid.NewGuid().ToString();
            const string field1 = "yyy";
            blobStorage.Write(id, new Dto { Field1 = field1 }, Timestamp.Now.Ticks, TimeSpan.FromSeconds(2));
            var elem = blobStorage.Read(id);
            Assert.AreEqual(field1, elem.Field1);
            Assert.That(() => blobStorage.Read(id), Is.Null.After(10000, 100));
        }

        private const string cfName = "LegacyBlobStorageTest";
        private LegacyBlobStorage<Dto> blobStorage;
        private readonly TimeSpan defaultTtl = TimeSpan.FromHours(1);

        private class Dto
        {
            public string Field1 { get; set; }
        }
    }
}