using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;
using SKBKontur.Catalogue.TestCore.Waiting;

#pragma warning disable 649

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    public class TaskSearchTests : SearchTasksTestBase
    {
        [Test]
        public void TestCreateNotDeserializedTaskData()
        {
            var t0 = DateTime.Now;
            var taskId = QueueTask(new SlowTaskData(), TimeSpan.FromSeconds(1));
            Console.WriteLine("TaskId: {0}", taskId);
            var taskMetaInformation = handleTasksMetaStorage.GetMeta(taskId);

            var badBytes = new byte[] {};
            Assert.Catch<Exception>(() => serializer.Deserialize<SlowTaskData>(badBytes));

            taskDataStorage.Overwrite(taskMetaInformation, badBytes);

            var taskId2 = QueueTask(new SlowTaskData(), TimeSpan.FromSeconds(2));

            WaitForTasks(new[] {taskId, taskId2}, TimeSpan.FromSeconds(5));

            var t1 = DateTime.Now;

            elasticMonitoringServiceClient.UpdateAndFlush();

            CheckSearch(string.Format("Meta.Id:\"{0}\"", taskId), t0, t1, taskId);
            CheckSearch(string.Format("Meta.Id:\"{0}\"", taskId2), t0, t1, taskId2);
        }

        [Test]
        public void TestStupid()
        {
            var t0 = DateTime.Now;

            var taskId = QueueTask(new SlowTaskData());
            WaitForTasks(new[] {taskId}, TimeSpan.FromSeconds(5));

            elasticMonitoringServiceClient.UpdateAndFlush();

            var t1 = DateTime.Now;
            CheckSearch("*", t0, t1, taskId);
            CheckSearch(string.Format("\"{0}\"", taskId), t0, t1, taskId);
            CheckSearch(string.Format("Meta.Id:\"{0}\"", taskId), t0, t1, taskId);
            CheckSearch(string.Format("Meta.Name:{0}", typeof(SlowTaskData).Name), t0, t1, taskId);
            CheckSearch("Meta.Name:Zzz", t0, t1);
        }

        [Test]
        public void TestSearchByExceptionSubstring()
        {
            var t0 = DateTime.Now;

            var uniqueData = Guid.NewGuid();
            Console.WriteLine("ud={0}", uniqueData);
            var taskId = QueueTask(new FailingTaskData {UniqueData = uniqueData, RetryCount = 0});
            WaitForTasks(new[] {taskId}, TimeSpan.FromSeconds(5));

            elasticMonitoringServiceClient.UpdateAndFlush();

            var t1 = DateTime.Now;
            CheckSearch(string.Format("\"{0}\"", taskId), t0, t1, taskId);
            CheckSearch(string.Format("\"{0}\"", uniqueData), t0, t1, taskId);
            CheckSearch(string.Format("ExceptionInfo:\"{0}\"", uniqueData), t0, t1, taskId);
            CheckSearch(string.Format("\"{0}\"", Guid.NewGuid()), t0, t1);
        }

        [Test]
        public void TestSearchByExpiration()
        {
            var t0 = DateTime.Now;

            var taskId = QueueTask(new SlowTaskData());
            WaitForTasks(new[] {taskId}, TimeSpan.FromSeconds(5));

            elasticMonitoringServiceClient.UpdateAndFlush();

            var t1 = DateTime.Now;
            var ttl = RemoteQueueTestsCassandraSettings.StandardTestTaskTtl;
            Console.WriteLine(ToIsoTime(new DateTime(remoteTaskQueue.GetTaskInfo<SlowTaskData>(taskId).Context.ExpirationTimestampTicks.Value, DateTimeKind.Utc)));
            CheckSearch(string.Format("Meta.ExpirationTime: [\"{0}\" TO \"{1}\"]", ToIsoTime(t0 + ttl), ToIsoTime(t1 + ttl + TimeSpan.FromSeconds(10))), t0, t1, taskId);
        }

        private static string ToIsoTime(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("O");
        }

        [Test]
        public void TestSearchByExceptionSubstring_MultipleDifferentErrors()
        {
            var t0 = DateTime.Now;

            var uniqueData = Guid.NewGuid();
            Console.WriteLine("ud={0}", uniqueData);
            var failingTaskData = new FailingTaskData {UniqueData = uniqueData, RetryCount = 2};
            var taskId = QueueTask(failingTaskData);
            WaitForTasks(new[] {taskId}, TimeSpan.FromSeconds(30));

            elasticMonitoringServiceClient.UpdateAndFlush();

            var t1 = DateTime.Now;
            CheckSearch(string.Format("\"{0}\"", taskId), t0, t1, taskId);
            CheckSearch(string.Format("\"{0}\"", uniqueData), t0, t1, taskId);
            CheckSearch(string.Format("\"{0}\"", Guid.NewGuid()), t0, t1);

            for(var attempts = 1; attempts <= 3; attempts++)
                CheckSearch(string.Format("\"FailingTask failed: {0}. Attempts = {1}\"", failingTaskData, attempts), t0, t1, taskId);
        }

        [Test]
        public void TestUpdateAndFlush()
        {
            var t0 = DateTime.Now;
            for(var i = 0; i < 100; i++)
            {
                Console.WriteLine("Iteration: {0}", i);
                var taskId0 = QueueTask(new SlowTaskData());
                WaitForTasks(new[] {taskId0}, TimeSpan.FromSeconds(5));
                elasticMonitoringServiceClient.UpdateAndFlush();
                CheckSearch(string.Format("\"{0}\"", taskId0), t0, DateTime.Now, taskId0);
            }
        }

        [Test]
        public void TestDataSearchBug()
        {
            var t0 = DateTime.Now;
            for(var i = 0; i < 100; i++)
            {
                Console.WriteLine("Iteration: {0}", i);
                var taskId0 = QueueTask(new SlowTaskData());
                var taskId1 = QueueTask(new SlowTaskData());
                var taskId2 = QueueTask(new SlowTaskData());
                //Console.WriteLine("ids: {0} {1} {2}", taskId0, taskId1, taskId2);
                WaitForTasks(new[] {taskId0, taskId1, taskId2}, TimeSpan.FromSeconds(5));
                elasticMonitoringServiceClient.UpdateAndFlush();
                CheckSearch(string.Format("\"{0}\"", taskId0), t0, DateTime.Now, taskId0);
            }
        }

        [Test]
        public void TestNotStupid()
        {
            var t0 = DateTime.Now;
            var taskId0 = QueueTask(new SlowTaskData());
            WaitForTasks(new[] {taskId0}, TimeSpan.FromSeconds(5));

            var t1 = DateTime.Now;

            var taskId1 = QueueTask(new AlphaTaskData());
            WaitForTasks(new[] {taskId1}, TimeSpan.FromSeconds(5));

            elasticMonitoringServiceClient.UpdateAndFlush();

            var t2 = DateTime.Now;

            CheckSearch("*", t0, t2, taskId0, taskId1);

            CheckSearch(string.Format("Meta.Id:\"{0}\" OR Meta.Id:\"{1}\"", taskId0, taskId1), t0, t1, taskId0, taskId1);
            CheckSearch("Meta.State:Finished", t0, t1, taskId0, taskId1);

            CheckSearch("NOT _exists_:Meta.ParentTaskId", t0, t1, taskId0, taskId1);
            CheckSearch("NOT _exists_:Meta.Name", t0, t1);

            CheckSearch("Data.TimeMs:0", t0, t1, taskId0);
            CheckSearch("Data.UseCounter:false", t0, t1, taskId0);
        }

        [Test]
        public void TestPaging()
        {
            var t0 = DateTime.Now;
            var lst = new List<string>();
            for(var i = 0; i < 200; i++)
                lst.Add(QueueTask(new SlowTaskData()));
            WaitForTasks(lst.ToArray(), TimeSpan.FromSeconds(60));
            elasticMonitoringServiceClient.UpdateAndFlush();

            var t1 = DateTime.Now;

            CheckSearch("*", t0, t1, lst.ToArray());
        }

        private string QueueTask<T>(T taskData, TimeSpan? delay = null) where T : ITaskData
        {
            var task = remoteTaskQueue.CreateTask(taskData);
            task.Queue(delay ?? TimeSpan.Zero);
            return task.Id;
        }

        private void WaitForTasks(IEnumerable<string> taskIds, TimeSpan timeSpan)
        {
            WaitHelper.Wait(() =>
                {
                    var tasks = remoteTaskQueue.HandleTaskCollection.GetTasks(taskIds.ToArray());
                    return tasks.All(t => t.Meta.State == TaskState.Finished || t.Meta.State == TaskState.Fatal) ? WaitResult.StopWaiting : WaitResult.ContinueWaiting;
                }, timeSpan);
        }

        [Injected]
        private ISerializer serializer;

        [Injected]
        private IHandleTasksMetaStorage handleTasksMetaStorage;

        [Injected]
        private TaskDataStorage taskDataStorage;
    }
}