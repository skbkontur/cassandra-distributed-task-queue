using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common;
using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.TimeBasedUuid;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests.RemoteTaskQueue.RepositoriesTests
{
    public class SinglePartitionTimeBasedBlobStorageTest : RepositoryFunctionalTestBase
    {
        [GroboSetUp]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void SetUp()
        {
            ResetCassandraState();
            timeBasedBlobStorage = new SinglePartitionTimeBasedBlobStorage(TestRtqSettings.QueueKeyspaceName, timeBasedCfName, cassandraCluster, new SilentLog());
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
            Assert.Throws<InvalidOperationException>(() => timeBasedBlobStorage.Write(NewRowKey(), NewBlobId(), null, Timestamp.Now.Ticks, defaultTtl));
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
            if (bytes == null)
                return null;
            return bytes.Single();
        }

        private const string timeBasedCfName = "SinglePartitionTimeBasedBlobStorageTestCf";

        private SinglePartitionTimeBasedBlobStorage timeBasedBlobStorage;
        private readonly TimeSpan defaultTtl = TimeSpan.FromHours(1);
    }
}