using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.TestCore;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    public class EventLogRepositoryTests : RepositoryFunctionalTestBase
    {
        protected override sealed ColumnFamily[] GetColumnFamilies()
        {
            return new[]
                {
                    new ColumnFamily {Name = TicksHolder.ColumnFamilyName},
                    new ColumnFamily {Name = EventLogRepository.ColumnFamilyName},
                };
        }

        [EdiSetUp]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void SetUp()
        {
            ClearEventLog();
            SetUpEventLog();
        }

        private void ClearEventLog()
        {
            ResetCassandraState();
            EdiTestContext.Current.Container.Get<TicksHolder>().ResetInMemoryState();
        }

        private void SetUpEventLog()
        {
            partition = TimeSpan.FromTicks(EventPointerFormatter.PartitionDurationTicks);

            var now = Timestamp.Now;
            timeSeriesStart = now - partition.Multiply(10);
            firstPartitionStart = new Timestamp(now.Ticks - now.Ticks % partition.Ticks);
            firstPartitionEnd = firstPartitionStart + partition - tick;
            secondPartitionStart = firstPartitionEnd + tick;
            secondPartitionEnd = secondPartitionStart + partition - tick;
            emptyPartitionStart = secondPartitionEnd + tick;
            emptyPartitionEnd = emptyPartitionStart + partition - tick;
            lastPartitionStart = emptyPartitionEnd + tick;
            lastPartitionEnd = lastPartitionStart + partition - tick;

            e0 = Event(timeSeriesStart, 0xff);
            e11 = Event(firstPartitionStart, 0x11);
            e12 = Event(firstPartitionStart, 0x12);
            e13 = Event(firstPartitionStart + second, 0x13);
            e14 = Event(firstPartitionStart + minute, 0x14);
            e15 = Event(firstPartitionEnd - minute, 0x15);
            e16 = Event(firstPartitionEnd - second, 0x16);
            e17 = Event(firstPartitionEnd, 0x17);
            e18 = Event(firstPartitionEnd, 0x18);
            e21 = Event(secondPartitionStart + second, 0x21);
            e22 = Event(secondPartitionStart + minute, 0x22);
            e23 = Event(secondPartitionEnd - minute, 0x23);
            e24 = Event(secondPartitionEnd - second, 0x24);
            ef1 = Event(lastPartitionStart, 0xf1);
            ef2 = Event(lastPartitionStart + second, 0xf2);
            ef3 = Event(lastPartitionEnd - second, 0xf3);
            ef4 = Event(lastPartitionEnd, 0xf4);
            WriteEvents(e0, e11, e12, e13, e14, e15, e16, e17, e18, e21, e22, e23, e24, ef1, ef2, ef3, ef4);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void GetEvents_EstimatedCount_IsNotPositive(int notPositiveEstimatedCount)
        {
            Action x = () => sut.GetEvents(null, OffsetInFarFuture(), notPositiveEstimatedCount);
            x.ShouldThrowExactly<InvalidProgramStateException>().WithMessage("estimatedCount <= 0");
        }

        [TestCase("")]
        [TestCase(null)]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void GetEvents_ToOffset_IsNotSet(string emptyToOffsetInclusive)
        {
            Action x = () => sut.GetEvents(null, emptyToOffsetInclusive, 1000);
            x.ShouldThrowExactly<InvalidProgramStateException>().WithMessage("toOffsetInclusive is not set");
        }

        [Test]
        public void GetEvents_ToOffset_IsLessThan_FromOffset()
        {
            var r = sut.GetEvents(e0.Offset, MinOffset(GetTimestamp(e0)), int.MaxValue);
            r.Events.Should().BeEmpty();
            r.LastOffset.Should().BeNull();
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_ToOffset_IsEqualTo_FromOffset()
        {
            var r = sut.GetEvents(e0.Offset, e0.Offset, int.MaxValue);
            r.Events.Should().BeEmpty();
            r.LastOffset.Should().BeNull();
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_NoEventsInLog()
        {
            ClearEventLog();
            var r = sut.GetEvents(null, OffsetInFarFuture(), int.MaxValue);
            r.Events.Should().BeEmpty();
            r.LastOffset.Should().BeNull();
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_NoEventsInRange()
        {
            var toOffsetInclusive = MaxOffset(GetTimestamp(e15) - tick);
            var r = sut.GetEvents(e14.Offset, toOffsetInclusive, int.MaxValue);
            r.Events.Should().BeEmpty();
            r.LastOffset.Should().Be(toOffsetInclusive);
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_SingleEventInLog()
        {
            ClearEventLog();
            WriteEvents(e0);
            var toOffsetInclusive = OffsetInFarFuture();

            var r = sut.GetEvents(null, toOffsetInclusive, int.MaxValue);
            r.Events.Single().ShouldBeEquivalentTo(e0);
            r.LastOffset.Should().Be(toOffsetInclusive);
            r.NoMoreEventsInSource.Should().BeTrue();

            r = sut.GetEvents(MaxOffset(timeSeriesStart - tick), toOffsetInclusive, int.MaxValue);
            r.Events.Single().ShouldBeEquivalentTo(e0);
            r.LastOffset.Should().Be(toOffsetInclusive);
            r.NoMoreEventsInSource.Should().BeTrue();

            r = sut.GetEvents(MinOffset(GetTimestamp(e0)), toOffsetInclusive, int.MaxValue);
            r.Events.Single().ShouldBeEquivalentTo(e0);
            r.LastOffset.Should().Be(toOffsetInclusive);
            r.NoMoreEventsInSource.Should().BeTrue();

            r = sut.GetEvents(e0.Offset, toOffsetInclusive, int.MaxValue);
            r.Events.Should().BeEmpty();
            r.LastOffset.Should().Be(toOffsetInclusive);
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_SameTimestamp()
        {
            var r = sut.GetEvents(MinOffset(GetTimestamp(e11)), MaxOffset(firstPartitionEnd), int.MaxValue);
            r.Events.ShouldBeEquivalentWithOrderTo(e11, e12, e13, e14, e15, e16, e17, e18);
            r.LastOffset.Should().Be(MaxOffset(firstPartitionEnd));
            r.NoMoreEventsInSource.Should().BeTrue();

            r = sut.GetEvents(e11.Offset, MaxOffset(firstPartitionEnd), int.MaxValue);
            r.Events.ShouldBeEquivalentWithOrderTo(e12, e13, e14, e15, e16, e17, e18);
            r.LastOffset.Should().Be(MaxOffset(firstPartitionEnd));
            r.NoMoreEventsInSource.Should().BeTrue();

            r = sut.GetEvents(MaxOffset(GetTimestamp(e11)), MaxOffset(firstPartitionEnd), int.MaxValue);
            r.Events.ShouldBeEquivalentWithOrderTo(e13, e14, e15, e16, e17, e18);
            r.LastOffset.Should().Be(MaxOffset(firstPartitionEnd));
            r.NoMoreEventsInSource.Should().BeTrue();

            r = sut.GetEvents(MinOffset(GetTimestamp(e17)), MaxOffset(firstPartitionEnd), int.MaxValue);
            r.Events.ShouldBeEquivalentWithOrderTo(e17, e18);
            r.LastOffset.Should().Be(MaxOffset(firstPartitionEnd));
            r.NoMoreEventsInSource.Should().BeTrue();

            r = sut.GetEvents(e17.Offset, MaxOffset(firstPartitionEnd), int.MaxValue);
            r.Events.ShouldBeEquivalentWithOrderTo(e18);
            r.LastOffset.Should().Be(MaxOffset(firstPartitionEnd));
            r.NoMoreEventsInSource.Should().BeTrue();

            r = sut.GetEvents(MaxOffset(GetTimestamp(e17)), MaxOffset(firstPartitionEnd), int.MaxValue);
            r.Events.Should().BeEmpty();
            r.LastOffset.Should().BeNull();
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_AllEvents()
        {
            var toOffsetInclusive = MaxOffset(lastPartitionEnd + partition + tick);
            var r = sut.GetEvents(MaxOffset(timeSeriesStart), toOffsetInclusive, int.MaxValue);
            r.Events.ShouldBeEquivalentWithOrderTo(e11, e12, e13, e14, e15, e16, e17, e18, e21, e22, e23, e24, ef1, ef2, ef3, ef4);
            r.LastOffset.Should().Be(toOffsetInclusive);
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_AllEvents_WithPaging()
        {
            var r = sut.GetEvents(e0.Offset, MaxOffset(lastPartitionEnd), 3);
            r.Events.ShouldBeEquivalentWithOrderTo(e11, e12, e13);
            r.LastOffset.Should().Be(e13.Offset);
            r.NoMoreEventsInSource.Should().BeFalse();

            r = sut.GetEvents(e13.Offset, MaxOffset(lastPartitionEnd), 3);
            r.Events.ShouldBeEquivalentWithOrderTo(e14, e15, e16);
            r.LastOffset.Should().Be(e16.Offset);
            r.NoMoreEventsInSource.Should().BeFalse();

            r = sut.GetEvents(e16.Offset, MaxOffset(lastPartitionEnd), 3);
            r.Events.ShouldBeEquivalentWithOrderTo(e17, e18, e21);
            r.LastOffset.Should().Be(e21.Offset);
            r.NoMoreEventsInSource.Should().BeFalse();

            r = sut.GetEvents(e21.Offset, MaxOffset(lastPartitionEnd), 3);
            r.Events.ShouldBeEquivalentWithOrderTo(e22, e23, e24);
            r.LastOffset.Should().Be(e24.Offset);
            r.NoMoreEventsInSource.Should().BeFalse();

            r = sut.GetEvents(e24.Offset, MaxOffset(lastPartitionEnd), 3);
            r.Events.ShouldBeEquivalentWithOrderTo(ef1, ef2, ef3);
            r.LastOffset.Should().Be(ef3.Offset);
            r.NoMoreEventsInSource.Should().BeFalse();

            r = sut.GetEvents(ef3.Offset, MaxOffset(lastPartitionEnd), 3);
            r.Events.ShouldBeEquivalentWithOrderTo(ef4);
            r.LastOffset.Should().Be(MaxOffset(lastPartitionEnd));
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_PartitionSwitch_OverEmptyPartition()
        {
            var r = sut.GetEvents(MaxOffset(firstPartitionEnd - tick), MaxOffset(lastPartitionStart), int.MaxValue);
            r.Events.ShouldBeEquivalentWithOrderTo(e17, e18, e21, e22, e23, e24, ef1);
            r.LastOffset.Should().Be(MaxOffset(lastPartitionStart));
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_ExclusiveStart_OnLowerPartitionBoundary_WithEventAtCursor()
        {
            var r = sut.GetEvents(Offset(firstPartitionStart, GetEventId(e11)), MaxOffset(lastPartitionEnd), 1);
            r.Events.Single().ShouldBeEquivalentTo(e12);
            r.LastOffset.Should().Be(e12.Offset);
            r.NoMoreEventsInSource.Should().BeFalse();
        }

        [Test]
        public void GetEvents_ExclusiveStart_OnLowerPartitionBoundary_WithNoEventAtCursor()
        {
            var r = sut.GetEvents(MinOffset(secondPartitionStart), MaxOffset(lastPartitionEnd), 1);
            r.Events.Single().ShouldBeEquivalentTo(e21);
            r.LastOffset.Should().Be(e21.Offset);
            r.NoMoreEventsInSource.Should().BeFalse();
        }

        [Test]
        public void GetEvents_ExclusiveStart_InsidePartition_WithEventAtCursor()
        {
            var r = sut.GetEvents(e14.Offset, MaxOffset(lastPartitionEnd), 1);
            r.Events.Single().ShouldBeEquivalentTo(e15);
            r.LastOffset.Should().Be(e15.Offset);
            r.NoMoreEventsInSource.Should().BeFalse();
        }

        [Test]
        public void GetEvents_ExclusiveStart_InsidePartition_WithNoEventAtCursor()
        {
            var r = sut.GetEvents(MinOffset(GetTimestamp(e14) + second), MaxOffset(lastPartitionEnd), 1);
            r.Events.Single().ShouldBeEquivalentTo(e15);
            r.LastOffset.Should().Be(e15.Offset);
            r.NoMoreEventsInSource.Should().BeFalse();
        }

        [Test]
        public void GetEvents_ExclusiveStart_OnUpperPartitionBoundary_WithEventAtCursor()
        {
            var r = sut.GetEvents(Offset(firstPartitionEnd, GetEventId(e18)), MaxOffset(lastPartitionEnd), 1);
            r.Events.Single().ShouldBeEquivalentTo(e21);
            r.LastOffset.Should().Be(e21.Offset);
            r.NoMoreEventsInSource.Should().BeFalse();
        }

        [Test]
        public void GetEvents_ExclusiveStart_OnUpperPartitionBoundary_WithNoEventAtCursor()
        {
            var r = sut.GetEvents(MinOffset(secondPartitionEnd), MaxOffset(lastPartitionEnd), 1);
            r.Events.Single().ShouldBeEquivalentTo(ef1);
            r.LastOffset.Should().Be(ef1.Offset);
            r.NoMoreEventsInSource.Should().BeFalse();
        }

        [Test]
        public void GetEvents_InclusiveEnd_OnLowerPartitionBoundary_WithEventAtCursor()
        {
            var r = sut.GetEvents(MinOffset(timeSeriesStart), MaxOffset(lastPartitionStart), int.MaxValue);
            r.Events.Last().ShouldBeEquivalentTo(ef1);
            r.LastOffset.Should().Be(MaxOffset(lastPartitionStart));
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_InclusiveEnd_OnLowerPartitionBoundary_WithNoEventAtCursor()
        {
            var r = sut.GetEvents(MinOffset(timeSeriesStart), MaxOffset(secondPartitionStart), int.MaxValue);
            r.Events.Last().ShouldBeEquivalentTo(e18);
            r.LastOffset.Should().Be(MaxOffset(secondPartitionStart));
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_InclusiveEnd_InsidePartition_WithEventAtCursor()
        {
            var r = sut.GetEvents(MinOffset(timeSeriesStart), MaxOffset(GetTimestamp(e15)), int.MaxValue);
            r.Events.Last().ShouldBeEquivalentTo(e15);
            r.LastOffset.Should().Be(MaxOffset(GetTimestamp(e15)));
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_InclusiveEnd_InsidePartition_WithNoEventAtCursor()
        {
            var r = sut.GetEvents(MinOffset(timeSeriesStart), MaxOffset(GetTimestamp(e15) - tick), int.MaxValue);
            r.Events.Last().ShouldBeEquivalentTo(e14);
            r.LastOffset.Should().Be(MaxOffset(GetTimestamp(e15) - tick));
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_InclusiveEnd_OnUpperPartitionBoundary_WithEventAtCursor()
        {
            var r = sut.GetEvents(e16.Offset, MaxOffset(firstPartitionEnd), int.MaxValue);
            r.Events.ShouldBeEquivalentWithOrderTo(e17, e18);
            r.LastOffset.Should().Be(MaxOffset(firstPartitionEnd));
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        [Test]
        public void GetEvents_InclusiveEnd_OnUpperPartitionBoundary_WithNoEventAtCursor()
        {
            var r = sut.GetEvents(MinOffset(timeSeriesStart), MaxOffset(secondPartitionEnd), int.MaxValue);
            r.Events.Last().ShouldBeEquivalentTo(e24);
            r.LastOffset.Should().Be(MaxOffset(secondPartitionEnd));
            r.NoMoreEventsInSource.Should().BeTrue();
        }

        private static EventWithOffset<TaskMetaUpdatedEvent, string> Event(Timestamp eventTimestamp, byte eventId)
        {
            var taskId = Guid.NewGuid().ToString();
            var offset = Offset(eventTimestamp, GuidTestingHelpers.Guid(eventId));
            return new EventWithOffset<TaskMetaUpdatedEvent, string>(new TaskMetaUpdatedEvent(taskId, eventTimestamp.Ticks), offset);
        }

        private static string OffsetInFarFuture()
        {
            return MaxOffset(Timestamp.Now.AddDays(1));
        }

        private static string MaxOffset(Timestamp timestamp)
        {
            return Offset(timestamp, GuidHelpers.MaxGuid);
        }

        private static string MinOffset(Timestamp timestamp)
        {
            return Offset(timestamp, GuidHelpers.MinGuid);
        }

        private static string Offset(Timestamp eventTimestamp, Guid eventId)
        {
            return EventPointerFormatter.GetColumnName(eventTimestamp.Ticks, eventId);
        }

        private void WriteEvents(params EventWithOffset<TaskMetaUpdatedEvent, string>[] events)
        {
            foreach (var eventWithOffset in events)
            {
                var taskMeta = new TaskMetaInformation("taskName", eventWithOffset.Event.TaskId);
                sut.AddEvent(taskMeta, GetTimestamp(eventWithOffset), GetEventId(eventWithOffset));
            }
        }

        private static Timestamp GetTimestamp(EventWithOffset<TaskMetaUpdatedEvent, string> eventWithOffset)
        {
            return EventPointerFormatter.GetTimestamp(eventWithOffset.Offset);
        }

        private static Guid GetEventId(EventWithOffset<TaskMetaUpdatedEvent, string> eventWithOffset)
        {
            return EventPointerFormatter.GetEventId(eventWithOffset.Offset);
        }

        [Injected]
        private readonly EventLogRepository sut;

        private readonly TimeSpan tick = TimeSpan.FromTicks(1);
        private readonly TimeSpan second = TimeSpan.FromSeconds(1);
        private readonly TimeSpan minute = TimeSpan.FromMinutes(1);

        private TimeSpan partition;
        private Timestamp timeSeriesStart;
        private EventWithOffset<TaskMetaUpdatedEvent, string> e0, e11, e12, e13, e14, e15, e16, e17, e18, e21, e22, e23, e24, ef1, ef2, ef3, ef4;
        private Timestamp firstPartitionStart, firstPartitionEnd, secondPartitionStart, secondPartitionEnd, emptyPartitionStart, emptyPartitionEnd, lastPartitionStart, lastPartitionEnd;
    }
}