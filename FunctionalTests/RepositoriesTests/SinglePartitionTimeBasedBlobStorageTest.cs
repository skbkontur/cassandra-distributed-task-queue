using System;
using System.Linq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace FunctionalTests.RepositoriesTests
{
    public class SinglePartitionTimeBasedBlobStorageTest : BlobStorageFunctionalTestBase
    {
        [SetUp]
        public void SetUp()
        {
            var cfName = new ColumnFamilyFullName(Container.Get<IRemoteTaskQueueSettings>().QueueKeyspace, timeBasedCfName);
            timeBasedBlobStorage = new SinglePartitionTimeBasedBlobStorage(cfName, Container.Get<ICassandraCluster>());
        }

        protected override ColumnFamily[] GetColumnFamilies()
        {
            return new[] {new ColumnFamily {Name = timeBasedCfName}};
        }

        [Test]
        public void Write()
        {
            var rk = NewRowKey();
            var id = NewBlobId();
            Assert.IsNull(timeBasedBlobStorage.Read(rk, id));
            WriteByte(rk, id, 10);
            Assert.That(ReadByte(rk, id), Is.EqualTo(10));
        }

        [Test]
        public void Write_NullValue_IsForbidden()
        {
            Assert.Throws<InvalidProgramStateException>(() => timeBasedBlobStorage.Write(NewRowKey(), NewBlobId(), null, DateTime.UtcNow.Ticks));
        }

        [Test]
        public void Write_EmptyValue()
        {
            var rk = NewRowKey();
            var id = NewBlobId();
            timeBasedBlobStorage.Write(rk, id, new byte[0], DateTime.UtcNow.Ticks);
            Assert.That(timeBasedBlobStorage.Read(rk, id), Is.EqualTo(new byte[0]));
        }

        [Test]
        public void Delete()
        {
            var rk = NewRowKey();
            var id = NewBlobId();
            WriteByte(rk, id, 1);
            timeBasedBlobStorage.Delete(rk, id, DateTime.UtcNow.Ticks);
            Assert.IsNull(ReadByte(rk, id));
        }

        [Test]
        public void Read_MultipleKeys()
        {
            var rk = NewRowKey();
            var id1 = NewBlobId();
            var id2 = NewBlobId();
            var allKeys = new[] {id1, id2, id1};

            Assert.That(timeBasedBlobStorage.Read(rk, new TimeGuid[0]), Is.Empty);
            Assert.That(timeBasedBlobStorage.Read(rk, allKeys), Is.Empty);

            WriteByte(rk, id1, 1);
            var result = timeBasedBlobStorage.Read(rk, allKeys);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[id1].Single(), Is.EqualTo(1));

            WriteByte(rk, id2, 2);
            result = timeBasedBlobStorage.Read(rk, allKeys);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[id1].Single(), Is.EqualTo(1));
            Assert.That(result[id2].Single(), Is.EqualTo(2));
        }

        private static string NewRowKey()
        {
            return Guid.NewGuid().ToString();
        }

        private static TimeGuid NewBlobId()
        {
            return TimeGuid.NowGuid();
        }

        private void WriteByte(string rowKey, TimeGuid id, byte value)
        {
            timeBasedBlobStorage.Write(rowKey, id, new[] {value}, DateTime.UtcNow.Ticks);
        }

        private byte? ReadByte(string rowKey, TimeGuid id)
        {
            var bytes = timeBasedBlobStorage.Read(rowKey, id);
            if(bytes == null)
                return null;
            return bytes.Single();
        }

        private const string timeBasedCfName = "SinglePartitionTimeBasedBlobStorageTestCf";

        private SinglePartitionTimeBasedBlobStorage timeBasedBlobStorage;
    }
}