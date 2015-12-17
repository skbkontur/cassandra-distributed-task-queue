using System;
using System.Collections.Generic;

using GroBuf;

using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace FunctionalTests.RepositoriesTests
{
    public class LegacyBlobStorageTest : BlobStorageFunctionalTestBase
    {
        [SetUp]
        public void SetUp()
        {
            var serializer = Container.Get<ISerializer>();
            var cassandraCluster = Container.Get<ICassandraCluster>();
            var keyspaceName = Container.Get<ICassandraSettings>().QueueKeyspace;
            blobStorage = new LegacyBlobStorage<Dto>(cassandraCluster, serializer, keyspaceName, cfName);
        }

        protected override ColumnFamily[] GetColumnFamilies()
        {
            return new[] {new ColumnFamily {Name = cfName}};
        }

        [Test]
        public void TestReadWrite()
        {
            const string id = "a";
            const string field1 = "yyy";
            blobStorage.Write(id, new Dto {Field1 = field1}, DateTime.UtcNow.Ticks);
            var elem = blobStorage.Read(id);
            Assert.AreEqual(field1, elem.Field1);
        }

        [Test]
        public void TestMultiRead()
        {
            var id1 = "9F7DF556-08BE-4E3E-8532-1489BF624657";
            var id2 = "AABAE964-1310-4842-B4ED-967F38796644";
            Assert.That(blobStorage.Read(new List<string>()).Count, Is.EqualTo(0));
            Assert.That(blobStorage.Read(new List<string> {id1, id2}).Count, Is.EqualTo(0));

            blobStorage.Write(id1, new Dto {Field1 = "id1"}, DateTime.UtcNow.Ticks);
            var actual = blobStorage.Read(new List<string> {id1, id2});
            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual[id1].Field1, Is.EqualTo("id1"));

            blobStorage.Write(id2, new Dto {Field1 = "id2"}, DateTime.UtcNow.Ticks);
            actual = blobStorage.Read(new List<string> {id1, id2});
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual[id1].Field1, Is.EqualTo("id1"));
            Assert.That(actual[id2].Field1, Is.EqualTo("id2"));
        }

        private const string cfName = "LegacyBlobStorageTest";
        private LegacyBlobStorage<Dto> blobStorage;

        private class Dto
        {
            public string Field1 { get; set; }
        }
    }
}