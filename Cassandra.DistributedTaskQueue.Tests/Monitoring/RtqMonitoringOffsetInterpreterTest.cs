using System;

using FluentAssertions;

using NUnit.Framework;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests.Monitoring
{
    [TestFixture]
    public class RtqMonitoringOffsetInterpreterTest
    {
        [Test]
        public void Compare_WithNull()
        {
            sut.Compare(null, null).Should().Be(0);
            sut.Compare(null, Offset(Timestamp.MinValue, minGuid)).Should().BeNegative();
            sut.Compare(Offset(Timestamp.MinValue, minGuid), null).Should().BePositive();
        }

        [Test]
        public void Compare_EqualOffsets()
        {
            var ts = Timestamp.Now;
            var eventId = Guid.NewGuid();
            sut.Compare(Offset(ts, eventId), Offset(ts, eventId)).Should().Be(0);
        }

        [Test]
        public void Compare_DifferentTimestamps()
        {
            var ts = Timestamp.Now;
            var eventId = Guid.NewGuid();
            sut.Compare(Offset(ts, eventId), Offset(ts.AddTicks(1), eventId)).Should().BeNegative();
        }

        [Test]
        public void Compare_EqualTimestamps()
        {
            var ts = Timestamp.Now;
            sut.Compare(Offset(ts, minGuid), Offset(ts, maxGuid)).Should().BeNegative();
            sut.Compare(Offset(ts, minGuid), Offset(ts, Guid.NewGuid())).Should().BeNegative();
            sut.Compare(Offset(ts, minGuid), Offset(ts, Guid.Parse("a0000000-0000-0000-0000-000000000000"))).Should().BeNegative();
            sut.Compare(Offset(ts, minGuid), Offset(ts, Guid.Parse("000000a0-0000-0000-0000-000000000000"))).Should().BeNegative();
            sut.Compare(Offset(ts, minGuid), Offset(ts, Guid.Parse("0000000a-0000-0000-0000-000000000000"))).Should().BeNegative();
            sut.Compare(Offset(ts, minGuid), Offset(ts, Guid.Parse("00000000-0000-0000-0000-00000000000a"))).Should().BeNegative();
            sut.Compare(Offset(ts, minGuid), Offset(ts, Guid.Parse("f0000000-0000-0000-0000-000000000000"))).Should().BeNegative();
            sut.Compare(Offset(ts, minGuid), Offset(ts, Guid.Parse("00000000-0000-0000-0000-00000000000f"))).Should().BeNegative();
            sut.Compare(Offset(ts, Guid.NewGuid()), Offset(ts, maxGuid)).Should().BeNegative();
            sut.Compare(Offset(ts, Guid.Parse("a0000000-0000-0000-0000-000000000000")), Offset(ts, maxGuid)).Should().BeNegative();
            sut.Compare(Offset(ts, Guid.Parse("000000a0-0000-0000-0000-000000000000")), Offset(ts, maxGuid)).Should().BeNegative();
            sut.Compare(Offset(ts, Guid.Parse("0000000a-0000-0000-0000-000000000000")), Offset(ts, maxGuid)).Should().BeNegative();
            sut.Compare(Offset(ts, Guid.Parse("00000000-0000-0000-0000-00000000000a")), Offset(ts, maxGuid)).Should().BeNegative();
            sut.Compare(Offset(ts, Guid.Parse("f0000000-0000-0000-0000-000000000000")), Offset(ts, maxGuid)).Should().BeNegative();
            sut.Compare(Offset(ts, Guid.Parse("00000000-0000-0000-0000-00000000000f")), Offset(ts, maxGuid)).Should().BeNegative();
        }

        [Test]
        public void GetTimestampFromOffset()
        {
            sut.GetTimestampFromOffset(null).Should().BeNull();
            sut.GetTimestampFromOffset(string.Empty).Should().BeNull();
            sut.GetTimestampFromOffset("00636207551966343582_7c314f94-26ea-4725-9cf8-ad5a91d01457").Should().Be(new Timestamp(636207551966343582));
        }

        [Test]
        public void GetMaxOffsetForTimestamp()
        {
            sut.GetMaxOffsetForTimestamp(new Timestamp(636207551966343582)).Should().Be("00636207551966343582_ffffffff-ffff-ffff-ffff-ffffffffffff");
        }

        private static string Offset(Timestamp ts, Guid eventId)
        {
            return EventPointerFormatter.GetColumnName(ts.Ticks, eventId);
        }

        private readonly RtqEventLogOffsetInterpreter sut = new RtqEventLogOffsetInterpreter();

        private readonly Guid minGuid = Guid.Empty;
        private readonly Guid maxGuid = EventPointerFormatter.MaxGuid;
    }
}