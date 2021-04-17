using System;

using FluentAssertions;

using NUnit.Framework;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests.UnitTests.Monitoring
{
    [TestFixture]
    public class EventPointerFormatterTest
    {
        [Test]
        public void PartitionDurationTicks()
        {
            EventPointerFormatter.PartitionDurationTicks.Should().Be(TimeSpan.FromMinutes(6).Ticks);
        }

        [Test]
        public void GetPartitionKey()
        {
            EventPointerFormatter.GetPartitionKey(636207551966343582).Should().Be("176724319");
            EventPointerFormatter.GetPartitionKey(636207548400000000).Should().Be("176724319");
            EventPointerFormatter.GetPartitionKey(new Timestamp(636207551966343582).Floor(TimeSpan.FromMinutes(6)).Ticks).Should().Be("176724319");
        }

        [Test]
        public void ParsePartitionKey()
        {
            EventPointerFormatter.ParsePartitionKey("176724319").Should().Be(176724319);
        }

        [Test]
        public void GetColumnName()
        {
            EventPointerFormatter.GetColumnName(636207551966343582, minGuid).Should().Be("00636207551966343582_00000000-0000-0000-0000-000000000000");
            EventPointerFormatter.GetColumnName(636207551966343582, maxGuid).Should().Be("00636207551966343582_ffffffff-ffff-ffff-ffff-ffffffffffff");
            EventPointerFormatter.GetColumnName(636207551966343582, Guid.Parse("7C314F94-26EA-4725-9CF8-AD5A91D01457")).Should().Be("00636207551966343582_7c314f94-26ea-4725-9cf8-ad5a91d01457");
        }

        [Test]
        public void GetTimestamp()
        {
            EventPointerFormatter.GetTimestamp("00636207551966343582_7c314f94-26ea-4725-9cf8-ad5a91d01457").Should().Be(new Timestamp(636207551966343582));
            EventPointerFormatter.GetTimestamp("00636207551966343582_ffffffff-ffff-ffff-ffff-ffffffffffff").Should().Be(new Timestamp(636207551966343582));
            EventPointerFormatter.GetTimestamp("00636207551966343582_00000000-0000-0000-0000-000000000000").Should().Be(new Timestamp(636207551966343582));
        }

        [Test]
        public void GetEventId()
        {
            EventPointerFormatter.GetEventId("00636207551966343582_7c314f94-26ea-4725-9cf8-ad5a91d01457").Should().Be(Guid.Parse("7c314f94-26ea-4725-9cf8-ad5a91d01457"));
            EventPointerFormatter.GetEventId("00636207551966343582_7C314F94-26EA-4725-9CF8-AD5A91D01457").Should().Be(Guid.Parse("7c314f94-26ea-4725-9cf8-ad5a91d01457"));
            EventPointerFormatter.GetEventId("00636207551966343582_ffffffff-ffff-ffff-ffff-ffffffffffff").Should().Be(maxGuid);
            EventPointerFormatter.GetEventId("00636207551966343582_00000000-0000-0000-0000-000000000000").Should().Be(minGuid);
        }

        [Test]
        public void CompareColumnNames_EqualColumnNames()
        {
            var ts = Timestamp.Now;
            var eventId = Guid.NewGuid();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, eventId), ColumnName(ts, eventId)).Should().Be(0);
        }

        [Test]
        public void CompareColumnNames_DifferentTimestamps()
        {
            var ts = Timestamp.Now;
            var eventId = Guid.NewGuid();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, eventId), ColumnName(ts.AddTicks(1), eventId)).Should().BeNegative();
        }

        [Test]
        public void CompareColumnNames_EqualTimestamps()
        {
            var ts = Timestamp.Now;
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, minGuid), ColumnName(ts, maxGuid)).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, minGuid), ColumnName(ts, Guid.NewGuid())).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, minGuid), ColumnName(ts, Guid.Parse("a0000000-0000-0000-0000-000000000000"))).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, minGuid), ColumnName(ts, Guid.Parse("000000a0-0000-0000-0000-000000000000"))).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, minGuid), ColumnName(ts, Guid.Parse("0000000a-0000-0000-0000-000000000000"))).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, minGuid), ColumnName(ts, Guid.Parse("00000000-0000-0000-0000-00000000000a"))).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, minGuid), ColumnName(ts, Guid.Parse("f0000000-0000-0000-0000-000000000000"))).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, minGuid), ColumnName(ts, Guid.Parse("00000000-0000-0000-0000-00000000000f"))).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, Guid.NewGuid()), ColumnName(ts, maxGuid)).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, Guid.Parse("a0000000-0000-0000-0000-000000000000")), ColumnName(ts, maxGuid)).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, Guid.Parse("000000a0-0000-0000-0000-000000000000")), ColumnName(ts, maxGuid)).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, Guid.Parse("0000000a-0000-0000-0000-000000000000")), ColumnName(ts, maxGuid)).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, Guid.Parse("00000000-0000-0000-0000-00000000000a")), ColumnName(ts, maxGuid)).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, Guid.Parse("f0000000-0000-0000-0000-000000000000")), ColumnName(ts, maxGuid)).Should().BeNegative();
            EventPointerFormatter.CompareColumnNames(ColumnName(ts, Guid.Parse("00000000-0000-0000-0000-00000000000f")), ColumnName(ts, maxGuid)).Should().BeNegative();
        }

        private static string ColumnName(Timestamp ts, Guid eventId)
        {
            return EventPointerFormatter.GetColumnName(ts.Ticks, eventId);
        }

        private readonly Guid minGuid = Guid.Empty;
        private readonly Guid maxGuid = EventPointerFormatter.MaxGuid;
    }
}