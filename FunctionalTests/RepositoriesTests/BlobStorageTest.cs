using GroBuf;

using NUnit.Framework;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Scheme;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace FunctionalTests.RepositoriesTests
{
    public class BlobStorageTest : FunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            var repositoryParameters = Container.Get<IColumnFamilyRepositoryParameters>();
            var serializer = Container.Get<ISerializer>();
            var globalTime = Container.Get<IGlobalTime>();
            var cassandraCluster = Container.Get<ICassandraCluster>();
            const string columnFamilyName = "Class1CF";
            connection = cassandraCluster.RetrieveKeyspaceConnection(settings.QueueKeyspace);
            cassandraCluster.ActualizeKeyspaces(new[]
                {
                    new KeyspaceScheme
                        {
                            Name = Container.Get<ICassandraSettings>().QueueKeyspace,
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
            blobStorage = new BlobStorage<Class1>(repositoryParameters, serializer, globalTime, columnFamilyName);
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
            blobStorage.Write(id, new Class1 {Field1 = field1});
            var elem = blobStorage.Read(id);
            Assert.AreEqual(field1, elem.Field1);
        }

        [Test]
        public void TestMultiRead()
        {
            var id1 = "9F7DF556-08BE-4E3E-8532-1489BF624657";
            var id2 = "AABAE964-1310-4842-B4ED-967F38796644";
            Assert.That(blobStorage.Read(new string[0]).Count, Is.EqualTo(0));
            Assert.That(blobStorage.Read(new[] {id1, id2}).Count, Is.EqualTo(0));

            blobStorage.Write(id1, new Class1 {Field1 = "id1"});
            var actual = blobStorage.Read(new[] {id1, id2});
            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual[id1].Field1, Is.EqualTo("id1"));

            blobStorage.Write(id2, new Class1 {Field1 = "id2"});
            actual = blobStorage.Read(new[] {id1, id2});
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual[id1].Field1, Is.EqualTo("id1"));
            Assert.That(actual[id2].Field1, Is.EqualTo("id2"));
        }

        private const string columnFamilyName = "Class1CF";
        private IBlobStorage<Class1> blobStorage;
        private IKeyspaceConnection connection;

        public class Class1
        {
            public string Field1 { get; set; }
        }
    }
}