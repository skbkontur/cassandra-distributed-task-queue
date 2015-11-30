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
    public class TimeBasedBlobStorageTest : FunctionalTestBaseWithoutServices
    {
        private const string columnFamilyName = "TimeBasedBlobStorageTest";

        public override void SetUp()
        {
            base.SetUp();
            var repositoryParameters = Container.Get<IColumnFamilyRepositoryParameters>();
            var serializer = Container.Get<ISerializer>();
            var globalTime = Container.Get<IGlobalTime>();
            var cassandraCluster = Container.Get<ICassandraCluster>();
            var settings = Container.Get<ICassandraSettings>();
            connection = cassandraCluster.RetrieveKeyspaceConnection(settings.QueueKeyspace);
            AddColumnFamily();

            timeBasedBlobStorage = new TimeBasedBlobStorage<int?>(repositoryParameters, serializer, globalTime, columnFamilyName);
        }

        public override void TearDown()
        {
            connection.RemoveColumnFamily(columnFamilyName);
            base.TearDown();
        }

        private void AddColumnFamily()
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
        public void TestReadWrite()
        {
            var id = TimeGuid.NowGuid();
            Assert.IsNull(timeBasedBlobStorage.Read(id));

            timeBasedBlobStorage.Write(id, 10);
            Assert.That(timeBasedBlobStorage.Read(id), Is.EqualTo(10));

            timeBasedBlobStorage.Write(id, 11);
            Assert.That(timeBasedBlobStorage.Read(id), Is.EqualTo(11));
        }

        [Test]
        public void TestMultiRead()
        {
            var id1 = TimeGuid.NowGuid();
            var id2 = TimeGuid.NowGuid();

            Assert.That(timeBasedBlobStorage.Read(new TimeGuid[0]).Count, Is.EqualTo(0));
            Assert.That(timeBasedBlobStorage.Read(new[] {id1, id2}).Count, Is.EqualTo(0));

            timeBasedBlobStorage.Write(id1, 1);
            var dictionary = timeBasedBlobStorage.Read(new[] {id1, id2});
            Assert.That(dictionary.Count, Is.EqualTo(1));
            Assert.That(dictionary[id1], Is.EqualTo(1));

            timeBasedBlobStorage.Write(id2, 2);
            dictionary = timeBasedBlobStorage.Read(new[] {id1, id2});
            Assert.That(dictionary.Count, Is.EqualTo(2));
            Assert.That(dictionary[id1], Is.EqualTo(1));
            Assert.That(dictionary[id2], Is.EqualTo(2));
        }

        [Test]
        public void TestDelete()
        {
            var id = TimeGuid.NowGuid();

            timeBasedBlobStorage.Write(id, 1);
            timeBasedBlobStorage.Delete(id, DateTime.UtcNow.Ticks);

            Assert.IsNull(timeBasedBlobStorage.Read(id));
        }

        [Test]
        public void TestMultiDelete()
        {
            var id1 = TimeGuid.NowGuid();
            var id2 = TimeGuid.NowGuid();

            timeBasedBlobStorage.Write(id1, 1);
            timeBasedBlobStorage.Write(id2, 2);

            timeBasedBlobStorage.Delete(new[] {id1, id2}, DateTime.UtcNow.Ticks);
            Assert.That(timeBasedBlobStorage.Read(new[] {id1, id2}).Count, Is.EqualTo(0));
        }

        [Test]
        public void TestReadAll()
        {
            var id11 = TimeGuid.NowGuid();
            var id12 = TimeGuid.NowGuid();

            var shift = new Timestamp(DateTime.UtcNow.AddDays(1));
            var id21 = TimeGuid.NewGuid(shift);
            var id22 = TimeGuid.NewGuid(shift);

            timeBasedBlobStorage.Write(id11, 11);
            timeBasedBlobStorage.Write(id12, 12);
            timeBasedBlobStorage.Write(id21, 21);
            timeBasedBlobStorage.Write(id22, 22);

            Assert.That(timeBasedBlobStorage.ReadAll(1).ToArray(), Is.EquivalentTo(new[]
                {
                    new KeyValuePair<TimeGuid, int?>(id11, 11),
                    new KeyValuePair<TimeGuid, int?>(id12, 12),
                    new KeyValuePair<TimeGuid, int?>(id21, 21),
                    new KeyValuePair<TimeGuid, int?>(id22, 22),
                }));
        }

        private TimeBasedBlobStorage<int?> timeBasedBlobStorage;
        private IKeyspaceConnection connection;
    }
}