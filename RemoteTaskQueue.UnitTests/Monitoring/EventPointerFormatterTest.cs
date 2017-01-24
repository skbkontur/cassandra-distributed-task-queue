using System;

using FluentAssertions;

using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories;

using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.UnitTests.Monitoring
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
            EventPointerFormatter.GetColumnName(636207551966343582, GuidHelpers.MinGuid).Should().Be("00636207551966343582_00000000-0000-0000-0000-000000000000");
            EventPointerFormatter.GetColumnName(636207551966343582, GuidHelpers.MaxGuid).Should().Be("00636207551966343582_ffffffff-ffff-ffff-ffff-ffffffffffff");
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
            EventPointerFormatter.GetEventId("00636207551966343582_ffffffff-ffff-ffff-ffff-ffffffffffff").Should().Be(GuidHelpers.MaxGuid);
            EventPointerFormatter.GetEventId("00636207551966343582_00000000-0000-0000-0000-000000000000").Should().Be(GuidHelpers.MinGuid);
        }
    }
}