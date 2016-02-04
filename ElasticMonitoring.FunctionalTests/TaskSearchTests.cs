using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Client;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Client;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

using TestCommon.NUnitWrappers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    [EdiTestSuite,
     WithApplicationSettings(FileName = "elasticMonitoringTests.csf"),
     WithExchangeServices,
     WithDefaultSerializer,
     WithCassandra("CatalogueCluster", "QueueKeyspace"),
     WithTestRemoteTaskQueue,
     WithRemoteLock("remoteLock")]
    public class TaskSearchTests
    {
        [EdiSetUp]
        public void SetUp()
        {
            WaitFor(() =>
                {
                    var status = elasticMonitoringServiceClient.GetStatus();
                    return status.DistributedLockAcquired;
                }, TimeSpan.FromMinutes(1));
            elasticMonitoringServiceClient.DeleteAll();

            taskSearchIndexSchema.ActualizeTemplate(true);
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
            CheckSearch(string.Format("Meta.Name:Zzz", typeof(SlowTaskData).Name), t0, t1, new string[0]);
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
            CheckSearch(string.Format("\"{0}\"", Guid.NewGuid()), t0, t1, new string[0]);
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
            CheckSearch(string.Format("\"{0}\"", Guid.NewGuid()), t0, t1, new string[0]);

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
            CheckSearch(string.Format("Meta.State:Finished"), t0, t1, taskId0, taskId1);

            CheckSearch(string.Format("_missing_:Meta.ParentTaskId"), t0, t1, taskId0, taskId1);
            CheckSearch(string.Format("_missing_:Meta.Name"), t0, t1);

            CheckSearch(string.Format("Data.TimeMs:0"), t0, t1, taskId0);
            CheckSearch(string.Format("Data.UseCounter:false"), t0, t1, taskId0);
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

        private void CheckSearch(string q, DateTime from, DateTime to, params string[] ids)
        {
            CollectionAssert.AreEquivalent(ids, Search(q, from, to), "q=" + q);
        }

        private string[] Search(string q, DateTime from, DateTime to)
        {
            //todo kill q2 and delete all ES data
            //var q2 = string.Format("({0}) AND (Meta.EnqueueTime:[\"{1}\" TO \"{2}\"])", q, ToIsoTime(@from), ToIsoTime(to));
            var taskSearchResponse = taskSearchClient.SearchFirst(new TaskSearchRequest()
                {
                    FromTicksUtc = @from.ToUniversalTime().Ticks,
                    ToTicksUtc = to.ToUniversalTime().Ticks,
                    QueryString = q
                });
            var result = new List<string>();
            if(taskSearchResponse.NextScrollId != null)
            {
                do
                {
                    foreach(var id in taskSearchResponse.Ids)
                        result.Add(id);
                    taskSearchResponse = taskSearchClient.SearchNext(taskSearchResponse.NextScrollId);
                } while(taskSearchResponse.Ids != null && taskSearchResponse.Ids.Length > 0);
            }
            return result.ToArray();
        }

        private string QueueTask<T>(T taskData) where T : ITaskData
        {
            var task = remoteTaskQueue.CreateTask<T>(taskData);
            task.Queue();
            return task.Id;
        }

        private static void WaitFor(Func<bool> func, TimeSpan timeout, int checkTimeout = 99)
        {
            var stopwatch = Stopwatch.StartNew();
            while(stopwatch.Elapsed < timeout)
            {
                Thread.Sleep(checkTimeout);
                if(func())
                    return;
            }
            Assert.Fail("Условия ожидания не выполнены за {0}", timeout);
        }

        private void WaitForTasks(IEnumerable<string> taskIds, TimeSpan timeSpan)
        {
            WaitFor(() => taskIds.All(taskId =>
                {
                    var taskState = remoteTaskQueue.GetTaskInfo(taskId).Context.State;
                    return taskState == TaskState.Finished || taskState == TaskState.Fatal;
                }), timeSpan);
        }

        [Injected]
        private readonly IElasticMonitoringServiceClient elasticMonitoringServiceClient;

        [Injected]
        private readonly TaskSearchIndexSchema taskSearchIndexSchema;

        [Injected]
        private readonly ITaskSearchClient taskSearchClient;

        [Injected]
        private readonly IRemoteTaskQueue remoteTaskQueue;
    }
}