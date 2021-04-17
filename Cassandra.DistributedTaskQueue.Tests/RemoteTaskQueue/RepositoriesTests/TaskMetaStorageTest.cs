using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages;
using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests.RemoteTaskQueue.RepositoriesTests
{
    public class TaskMetaStorageTest : RepositoryFunctionalTestBase
    {
        [GroboSetUp]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void SetUp()
        {
            ResetCassandraState();
        }

        protected override ColumnFamily[] GetColumnFamilies()
        {
            return TaskMetaStorage.GetColumnFamilyNames().Select(x => new ColumnFamily {Name = x}).ToArray();
        }

        [Test]
        public void Write()
        {
            var taskId = TaskId();
            Assert.IsNull(taskMetaStorage.Read(taskId));
            Write(taskId);
            Assert.That(taskMetaStorage.Read(taskId).Id, Is.EqualTo(taskId));
        }

        [Test]
        public void Delete()
        {
            var taskId = TaskId();
            Write(taskId);
            taskMetaStorage.Delete(taskId, Timestamp.Now.Ticks);
            Assert.IsNull(taskMetaStorage.Read(taskId));
        }

        [Test]
        public void Read_MultipleTaskIds()
        {
            var taskId1 = TaskId();
            var taskId2 = TaskId();
            var allTaskIds = new[] {taskId1, taskId2};

            Assert.That(taskMetaStorage.Read(new string[0]), Is.Empty);
            Assert.That(taskMetaStorage.Read(allTaskIds), Is.Empty);

            Write(taskId1);
            var result = taskMetaStorage.Read(allTaskIds);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[taskId1].Id, Is.EqualTo(taskId1));

            Write(taskId2);
            result = taskMetaStorage.Read(allTaskIds);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[taskId1].Id, Is.EqualTo(taskId1));
            Assert.That(result[taskId2].Id, Is.EqualTo(taskId2));
        }

        [Test]
        public void ReadAll()
        {
            var taskId1 = TaskId();
            var taskId2 = TaskId();
            Write(taskId1);
            Write(taskId2);
            Assert.That(taskMetaStorage.ReadAll(1).Select(x => Tuple.Create(x.Item1, x.Item2.Id)).ToArray(),
                        Is.EquivalentTo(new[]
                            {
                                new Tuple<string, string>(taskId1, taskId1),
                                new Tuple<string, string>(taskId2, taskId2),
                            }));
        }

        [Test]
        public void Ttl_TimeBased()
        {
            var taskId = TaskId();
            Assert.IsNull(taskMetaStorage.Read(taskId));
            var ttl = TimeSpan.FromSeconds(2);
            Write(taskId, ttl);
            var taskMetaInformation = taskMetaStorage.Read(taskId);
            Assert.That(taskMetaInformation.Id, Is.EqualTo(taskId));
            Assert.That(() => taskMetaStorage.Read(taskId), Is.Null.After(10000, 100));
        }

        private static string TaskId()
        {
            return TimeGuid.NowGuid().ToGuid().ToString();
        }

        private void Write(string taskId, TimeSpan? ttl = null)
        {
            var now = Timestamp.Now;
            var taskMeta = new TaskMetaInformation("TaskName", taskId) {MinimalStartTicks = now.Ticks};
            taskMeta.SetOrUpdateTtl(ttl ?? defaultTtl);
            taskMetaStorage.Write(taskMeta, now.Ticks);
        }

        [Injected]
        private readonly TaskMetaStorage taskMetaStorage;

        private readonly TimeSpan defaultTtl = TimeSpan.FromHours(1);
    }
}