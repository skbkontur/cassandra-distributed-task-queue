using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.BlobStorages;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    public class SinglePartitionTimeBasedBlobStorageTest : BlobStorageFunctionalTestBase
    {
        [EdiSetUp]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void SetUp()
        {
            ResetCassandraState();
            var cfName = new ColumnFamilyFullName(QueueKeyspaceName, timeBasedCfName);
            timeBasedBlobStorage = new SinglePartitionTimeBasedBlobStorage(cfName, cassandraCluster);
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
            WriteByte(rk, id, 10, defaultTtl);
            Assert.That(ReadByte(rk, id), Is.EqualTo(10));
        }

        [Test]
        public void Write_NullValue_IsForbidden()
        {
            Assert.Throws<InvalidProgramStateException>(() => timeBasedBlobStorage.Write(NewRowKey(), NewBlobId(), null, Timestamp.Now.Ticks, defaultTtl));
        }

        [Test]
        public void Write_EmptyValue()
        {
            var rk = NewRowKey();
            var id = NewBlobId();
            timeBasedBlobStorage.Write(rk, id, new byte[0], Timestamp.Now.Ticks, defaultTtl);
            Assert.That(timeBasedBlobStorage.Read(rk, id), Is.EqualTo(new byte[0]));
        }

        [Test]
        public void Delete()
        {
            var rk = NewRowKey();
            var id = NewBlobId();
            WriteByte(rk, id, 1, defaultTtl);
            timeBasedBlobStorage.Delete(rk, id, Timestamp.Now.Ticks);
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

            WriteByte(rk, id1, 1, defaultTtl);
            var result = timeBasedBlobStorage.Read(rk, allKeys);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[id1].Single(), Is.EqualTo(1));

            WriteByte(rk, id2, 2, defaultTtl);
            result = timeBasedBlobStorage.Read(rk, allKeys);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[id1].Single(), Is.EqualTo(1));
            Assert.That(result[id2].Single(), Is.EqualTo(2));
        }

        [Test]
        public void Ttl()
        {
            var rowKey = NewRowKey();
            var id = NewBlobId();
            Assert.IsNull(ReadByte(rowKey, id));
            WriteByte(rowKey, id, 10, TimeSpan.FromSeconds(2));
            Assert.That(ReadByte(rowKey, id), Is.EqualTo(10));
            Assert.That(() => ReadByte(rowKey, id), Is.Null.After(10000, 1000));
        }

        private static string NewRowKey()
        {
            return Guid.NewGuid().ToString();
        }

        private static TimeGuid NewBlobId()
        {
            return TimeGuid.NowGuid();
        }

        private void WriteByte(string rowKey, TimeGuid id, byte value, TimeSpan ttl)
        {
            timeBasedBlobStorage.Write(rowKey, id, new[] {value}, Timestamp.Now.Ticks, ttl);
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
        private readonly TimeSpan defaultTtl = TimeSpan.FromHours(1);
    }
}