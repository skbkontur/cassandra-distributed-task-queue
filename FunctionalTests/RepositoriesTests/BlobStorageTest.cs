using GroBuf;

using NUnit.Framework;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Scheme;

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

        [Test]
        public void SimpleTest()
        {
            const string id = "a";
            const string field1 = "yyy";
            blobStorage.Write(id, new Class1 {Field1 = field1});
            var elem = blobStorage.Read(id);
            Assert.AreEqual(field1, elem.Field1);
        }

        private IBlobStorage<Class1> blobStorage;

        public class Class1
        {
            public string Field1 { get; set; }
        }
    }
}