using System;
using System.Collections.Generic;

using GroBuf;

using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Connections;
using SKBKontur.Cassandra.CassandraClient.Scheme;

namespace FunctionalTests.RepositoriesTests
{
    public class LegacyBlobStorageTest : FunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            var serializer = Container.Get<ISerializer>();
            var cassandraCluster = Container.Get<ICassandraCluster>();
            const string columnFamilyName = "Class1CF";
            var queueKeyspace = Container.Get<ICassandraSettings>().QueueKeyspace;
            connection = cassandraCluster.RetrieveKeyspaceConnection(queueKeyspace);
            cassandraCluster.ActualizeKeyspaces(new[]
                {
                    new KeyspaceScheme
                        {
                            Name = queueKeyspace,
                            Configuration =
                                {
                                    ReplicationFactor = 1,
                                    ReplicaPlacementStrategy = ReplicaPlacementStrategy.Simple,
                                    ColumnFamilies = new[]
                                        {
                                            new ColumnFamily
                                                {
                                                    Name = columnFamilyName,
                                                    GCGraceSeconds = 10,
                                                    Caching = ColumnFamilyCaching.All,
                                                }
                                        },
                                }
                        },
                });
            blobStorage = new LegacyBlobStorage<Class1>(cassandraCluster, serializer, queueKeyspace, columnFamilyName);
        }

        public override void TearDown()
        {
            connection.RemoveColumnFamily(columnFamilyName);
            base.TearDown();
        }

        [Test]
        public void TestReadWrite()
        {
            const string id = "a";
            const string field1 = "yyy";
            blobStorage.Write(id, new Class1 {Field1 = field1}, DateTime.UtcNow.Ticks);
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

            blobStorage.Write(id1, new Class1 {Field1 = "id1"}, DateTime.UtcNow.Ticks);
            var actual = blobStorage.Read(new List<string> {id1, id2});
            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual[id1].Field1, Is.EqualTo("id1"));

            blobStorage.Write(id2, new Class1 {Field1 = "id2"}, DateTime.UtcNow.Ticks);
            actual = blobStorage.Read(new List<string> {id1, id2});
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual[id1].Field1, Is.EqualTo("id1"));
            Assert.That(actual[id2].Field1, Is.EqualTo("id2"));
        }

        private const string columnFamilyName = "Class1CF";
        private LegacyBlobStorage<Class1> blobStorage;
        private IKeyspaceConnection connection;

        public class Class1
        {
            public string Field1 { get; set; }
        }
    }
}