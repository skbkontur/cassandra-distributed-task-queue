using System;
using System.Linq;

using GroBuf;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace FunctionalTests.RepositoriesTests
{
    public class TaskMetaStorageTest : BlobStorageFunctionalTestBase
    {
        [SetUp]
        public void SetUp()
        {
            taskMetaStorage = Container.Get<TaskMetaStorage>();
        }

        protected override ColumnFamily[] GetColumnFamilies()
        {
            return TaskMetaStorage.GetColumnFamilyNames().Select(x => new ColumnFamily {Name = x}).ToArray();
        }

        [Test]
        public void Write_TimeBasedMeta()
        {
            TestWrite(TimeBasedTaskId());
        }

        [Test]
        public void Write_LegacyMeta()
        {
            TestWrite(LegacyTaskId());
        }

        private void TestWrite(string taskId)
        {
            Assert.IsNull(taskMetaStorage.Read(taskId));
            Write(taskId);
            Assert.That(taskMetaStorage.Read(taskId).Id, Is.EqualTo(taskId));
        }

        [Test]
        public void Delete_TimeBasedMeta()
        {
            TestDelete(TimeBasedTaskId());
        }

        [Test]
        public void Delete_LegacyMeta()
        {
            TestDelete(LegacyTaskId());
        }

        private void TestDelete(string taskId)
        {
            Write(taskId);
            taskMetaStorage.Delete(taskId, DateTime.UtcNow.Ticks);
            Assert.IsNull(taskMetaStorage.Read(taskId));
        }

        [Test]
        public void Read_MultipleTaskIds()
        {
            var taskId1 = TimeBasedTaskId();
            var taskId2 = LegacyTaskId();
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
            var taskId1 = TimeBasedTaskId();
            var taskId2 = LegacyTaskId();
            Write(taskId1);
            Write(taskId2);
            Assert.That(taskMetaStorage.ReadAll(1).Select(x => Tuple.Create(x.Item1, x.Item2.Id)).ToArray(), Is.EquivalentTo(new[]
                {
                    new Tuple<string, string>(taskId1, taskId1),
                    new Tuple<string, string>(taskId2, taskId2),
                }));
        }

        [Test]
        public void Ttl_TimeBased()
        {
            DoTestTtl(TimeBasedTaskId());
        }
        
        [Test]
        public void Ttl_Legacy()
        {
            DoTestTtl(LegacyTaskId());
        }

        private void DoTestTtl(string taskId)
        {
            var ttl = TimeSpan.FromSeconds(2);
            taskMetaStorage = new TaskMetaStorage(
                Container.Get<ICassandraCluster>(),
                Container.Get<ISerializer>(),
                new SmallTtlRemoteTaskQueueSettings(Container.Get<IRemoteTaskQueueSettings>(), ttl));

            Assert.IsNull(taskMetaStorage.Read(taskId));
            Write(taskId);
            var taskMetaInformation = taskMetaStorage.Read(taskId);
            Assert.That(taskMetaInformation.Id, Is.EqualTo(taskId));
            Assert.That(taskMetaInformation.TtlTicks, Is.EqualTo(ttl.Ticks));
            Assert.That(() => taskMetaStorage.Read(taskId), Is.Null.After(10000, 100));
        }

        private static string LegacyTaskId()
        {
            return Guid.NewGuid().ToString();
        }

        private static string TimeBasedTaskId()
        {
            return TimeGuid.NowGuid().ToGuid().ToString();
        }

        private void Write(string taskId)
        {
            taskMetaStorage.Write(new TaskMetaInformation("TaskName", taskId), DateTime.UtcNow.Ticks);
        }

        private TaskMetaStorage taskMetaStorage;
    }
}