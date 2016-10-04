using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteQueue.Tests
{
    [TestFixture]
    public class TaskMetaInformationTest
    {
        [Test]
        public void ExceptionInfoIdsLimit()
        {
            Assert.That(TaskMetaInformation.TaskExceptionIfoIdsLimit, Is.EqualTo(201));
            var meta = new TaskMetaInformation("TaskName", Guid.NewGuid().ToString());
            List<TimeGuid> newExceptionInfoIds;
            TimeGuid newExceptionInfoId, oldExceptionInfoId;
            for(ushort i = 0; i < 201; i++)
            {
                newExceptionInfoId = NewExceptionInfoId(i);
                newExceptionInfoIds = meta.AddExceptionInfoId(newExceptionInfoId, out oldExceptionInfoId);
                Assert.That(oldExceptionInfoId, Is.Null);
                Assert.That(newExceptionInfoIds.Select(x => (int)x.GetClockSequence()).ToArray(), Is.EqualTo(Enumerable.Range(0, i + 1).ToArray()));
                meta.TaskExceptionInfoIds = newExceptionInfoIds;
            }
            newExceptionInfoId = NewExceptionInfoId(999);
            newExceptionInfoIds = meta.AddExceptionInfoId(newExceptionInfoId, out oldExceptionInfoId);
            Assert.That(oldExceptionInfoId.GetClockSequence(), Is.EqualTo(100));
            Assert.That(newExceptionInfoIds.Select(x => (int)x.GetClockSequence()).ToArray(), Is.EqualTo(Enumerable.Range(0, 100).Concat(Enumerable.Range(101, 100)).Concat(new []{999}).ToArray()));
        }

        [Test]
        public void SetOrUpdateTtl_Now()
        {
            var now = Timestamp.Now;
            var meta = new TaskMetaInformation("Test_name", "Test-id");
            var ttl = TimeSpan.FromMilliseconds(3342);
            meta.SetOrUpdateTtl(ttl);
            Assert.That(meta.GetTtl(), Is.InRange(ttl - TimeSpan.FromSeconds(1), ttl));
            Assert.That(meta.ExpirationTimestampTicks, Is.GreaterThanOrEqualTo((now + ttl).Ticks));
        }

        [Test]
        public void SetOrUpdateTtl_MinimalStartTicks()
        {
            var minimalStart = Timestamp.Now + TimeSpan.FromHours(1);
            var ttl = TimeSpan.FromMilliseconds(3342);
            var meta = new TaskMetaInformation("Test_name", "Test-id"){MinimalStartTicks = minimalStart.Ticks};
            meta.SetOrUpdateTtl(ttl);
            Assert.That(meta.GetTtl(), Is.InRange(TimeSpan.FromHours(1) - ttl, TimeSpan.FromHours(1) + ttl));
            Assert.That(meta.ExpirationTimestampTicks, Is.GreaterThanOrEqualTo((minimalStart + ttl).Ticks));
        }

        private static TimeGuid NewExceptionInfoId(ushort c)
        {
            return TimeGuid.NewGuid(Timestamp.Now, c);
        }
    }
}