using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.BlobStorages;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    public class TimeBasedBlobStorageTest : RepositoryFunctionalTestBase
    {
        [EdiSetUp]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void SetUp()
        {
            ResetCassandraState();
            var settings = new TimeBasedBlobStorageSettings(QueueKeyspaceName, largeCfName, regularCfName);
            timeBasedBlobStorage = new TimeBasedBlobStorage(settings, cassandraCluster);
        }

        protected override ColumnFamily[] GetColumnFamilies()
        {
            return new[]
                {
                    new ColumnFamily {Name = largeCfName},
                    new ColumnFamily {Name = regularCfName}
                };
        }

        [Test]
        public void GenerateNewBlobId()
        {
            Assert.That(TimeBasedBlobStorage.GenerateNewBlobId(0).Type, Is.EqualTo(BlobType.Regular));
            Assert.That(TimeBasedBlobStorage.GenerateNewBlobId(TimeBasedBlobStorageSettings.MaxRegularBlobSize).Type, Is.EqualTo(BlobType.Regular));
            Assert.That(TimeBasedBlobStorage.GenerateNewBlobId(TimeBasedBlobStorageSettings.MaxRegularBlobSize + 1).Type, Is.EqualTo(BlobType.Large));
            Assert.That(TimeBasedBlobStorage.GenerateNewBlobId(int.MaxValue).Type, Is.EqualTo(BlobType.Large));
        }

        [Test]
        public void Write_Regular()
        {
            var id = RegularBlobId();
            Assert.IsNull(timeBasedBlobStorage.Read(id));
            WriteByte(id, 10);
            Assert.That(ReadByte(id), Is.EqualTo(10));
        }

        [Test]
        public void Write_Large()
        {
            var id = LargeBlobId();
            Assert.IsNull(timeBasedBlobStorage.Read(id));
            WriteByte(id, 11);
            Assert.That(ReadByte(id), Is.EqualTo(11));
        }

        [Test]
        public void Write_NullValue_IsForbidden()
        {
            Assert.Throws<InvalidProgramStateException>(() => timeBasedBlobStorage.Write(RegularBlobId(), null, Timestamp.Now.Ticks, TimeSpan.FromHours(1)));
        }

        [Test]
        public void Write_EmptyValue()
        {
            var id = RegularBlobId();
            timeBasedBlobStorage.Write(id, new byte[0], Timestamp.Now.Ticks, TimeSpan.FromHours(1));
            Assert.That(timeBasedBlobStorage.Read(id), Is.EqualTo(new byte[0]));
        }

        [Test]
        public void Write_Regular_WithLargeValue()
        {
            var id = RegularBlobId();
            var rng = new Random();
            var value = rng.NextBytes(TimeBasedBlobStorageSettings.MaxRegularBlobSize + 1);
            timeBasedBlobStorage.Write(id, value, Timestamp.Now.Ticks, TimeSpan.FromHours(1));
            Assert.That(timeBasedBlobStorage.Read(id), Is.EqualTo(value));
        }

        [Test]
        public void Delete()
        {
            var id = RegularBlobId();
            WriteByte(id, 1);
            timeBasedBlobStorage.Delete(id, Timestamp.Now.Ticks);
            Assert.IsNull(ReadByte(id));
        }

        [Test]
        public void Read_MultipleKeys()
        {
            var id1 = RegularBlobId();
            var id2 = RegularBlobId();
            var id3 = LargeBlobId();
            var allKeys = new[] {id1, id2, id2, id3, id3};

            Assert.That(timeBasedBlobStorage.Read(new BlobId[0]), Is.Empty);
            Assert.That(timeBasedBlobStorage.Read(allKeys), Is.Empty);

            WriteByte(id1, 1);
            var result = timeBasedBlobStorage.Read(allKeys);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[id1].Single(), Is.EqualTo(1));

            WriteByte(id2, 2);
            result = timeBasedBlobStorage.Read(allKeys);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[id1].Single(), Is.EqualTo(1));
            Assert.That(result[id2].Single(), Is.EqualTo(2));

            WriteByte(id3, 3);
            result = timeBasedBlobStorage.Read(allKeys);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[id1].Single(), Is.EqualTo(1));
            Assert.That(result[id2].Single(), Is.EqualTo(2));
            Assert.That(result[id3].Single(), Is.EqualTo(3));
        }

        [Test]
        public void Read_MultipleKeys_DifferentRows()
        {
            var id1 = RegularBlobId(TimeGuid.NewGuid(Timestamp.Now.Add(TimeSpan.FromDays(1))));
            var id2 = RegularBlobId(TimeGuid.NewGuid(Timestamp.Now.Add(TimeSpan.FromDays(-1))));
            var id3 = LargeBlobId();
            var id4 = LargeBlobId();
            var allKeys = new[] {id1, id2, id3, id4};
            WriteByte(id1, 1);
            WriteByte(id2, 2);
            WriteByte(id3, 3);
            WriteByte(id4, 4);
            var result = timeBasedBlobStorage.Read(allKeys);
            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result[id1].Single(), Is.EqualTo(1));
            Assert.That(result[id2].Single(), Is.EqualTo(2));
            Assert.That(result[id3].Single(), Is.EqualTo(3));
            Assert.That(result[id4].Single(), Is.EqualTo(4));
        }

        [Test]
        public void ReadAll()
        {
            var idLarge = LargeBlobId();
            var id11 = RegularBlobId();
            var id12 = RegularBlobId();
            var shift = Timestamp.Now.AddDays(1);
            var id21 = RegularBlobId(TimeGuid.NewGuid(shift));
            var id22 = RegularBlobId(TimeGuid.NewGuid(shift));

            WriteByte(idLarge, 255);
            WriteByte(id11, 11);
            WriteByte(id12, 12);
            WriteByte(id21, 21);
            WriteByte(id22, 22);

            Assert.That(timeBasedBlobStorage.ReadAll(1).Select(x => Tuple.Create(x.Item1, x.Item2.Single())).ToArray(), Is.EquivalentTo(new[]
                {
                    new Tuple<BlobId, byte>(idLarge, 255),
                    new Tuple<BlobId, byte>(id11, 11),
                    new Tuple<BlobId, byte>(id12, 12),
                    new Tuple<BlobId, byte>(id21, 21),
                    new Tuple<BlobId, byte>(id22, 22),
                }));
        }

        [Test]
        public void Ttl_Regular()
        {
            DoTestTtl(RegularBlobId());
        }

        [Test]
        public void Ttl_Large()
        {
            DoTestTtl(LargeBlobId());
        }

        private void DoTestTtl(BlobId id)
        {
            Assert.IsNull(ReadByte(id));
            WriteByte(id, 10, TimeSpan.FromSeconds(2));
            Assert.That(ReadByte(id), Is.EqualTo(10));
            Assert.That(() => ReadByte(id), Is.Null.After(10000, 1000));
        }

        private static BlobId RegularBlobId(TimeGuid timeGuid = null)
        {
            return new BlobId(timeGuid ?? TimeGuid.NowGuid(), BlobType.Regular);
        }

        private static BlobId LargeBlobId()
        {
            return new BlobId(TimeGuid.NowGuid(), BlobType.Large);
        }

        private void WriteByte(BlobId id, byte value, TimeSpan? ttl = null)
        {
            ttl = ttl ?? TimeSpan.FromHours(1);
            timeBasedBlobStorage.Write(id, new[] {value}, Timestamp.Now.Ticks, ttl.Value);
        }

        private byte? ReadByte(BlobId id)
        {
            var bytes = timeBasedBlobStorage.Read(id);
            if(bytes == null)
                return null;
            return bytes.Single();
        }

        private const string largeCfName = "TimeBasedBlobStorageTest_LargeCf";
        private const string regularCfName = "TimeBasedBlobStorageTest_RegularCf";

        private TimeBasedBlobStorage timeBasedBlobStorage;
    }
}