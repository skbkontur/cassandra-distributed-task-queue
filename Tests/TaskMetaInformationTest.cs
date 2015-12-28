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

        private static TimeGuid NewExceptionInfoId(ushort c)
        {
            return TimeGuid.NewGuid(Timestamp.Now, c);
        }
    }
}