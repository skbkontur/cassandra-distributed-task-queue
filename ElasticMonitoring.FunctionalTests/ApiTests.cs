using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.Ranges;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Api;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Client;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

using TestCommon;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    [EdiTestSuite("ElasticMonitoringTestSuite"), WithColumnFamilies, WithExchangeServices, WithApplicationSettings(FileName = "elasticMonitoringTests.csf")]
    public class ApiTests
    {
        [EdiSetUp]
        public void SetUp()
        {
            TaskSearchHelpers.WaitFor(() =>
                {
                    var status = elasticMonitoringServiceClient.GetStatus();
                    return status.DistributedLockAcquired;
                }, TimeSpan.FromMinutes(1));
            elasticMonitoringServiceClient.DeleteAll();
            taskSearchIndexSchema.ActualizeTemplate(true);
        }

        [Test]
        public void TestSearchByRange()
        {
            var t0 = DateTime.Now;
            var taskIds = QueueTasksAndWaitForActualization(
                Enumerable.Range(0, 20)
                          .Select(x => new AlphaTaskData()).Cast<ITaskData>().ToArray());
            var t1 = DateTime.Now;
            var searchResults = remoteTaskQueueApiImpl.Search(new RemoteTaskQueueSearchRequest
                {
                    EnqueueDateTimeRange = Range.OfDate(t0, t1),
                });
            searchResults.TotalCount.Should().Be(20);
            searchResults.TaskMetas.Select(x => x.Id).ShouldAllBeEquivalentTo(taskIds);
        }

        [Test]
        public void TestByTaskNames()
        {
            var t0 = DateTime.Now;
            QueueTasksAndWaitForActualization(
                new AlphaTaskData(),
                new BetaTaskData(),
                new DeltaTaskData());
            var t1 = DateTime.Now;
            var searchResults1 = remoteTaskQueueApiImpl.Search(new RemoteTaskQueueSearchRequest
                {
                    EnqueueDateTimeRange = Range.OfDate(t0, t1),
                    Names = new[] {typeof(AlphaTaskData).Name}
                });
            searchResults1.TotalCount.Should().Be(1);
            searchResults1.TaskMetas.Single().Name.Should().Be(typeof(AlphaTaskData).Name);

            var searchResults2 = remoteTaskQueueApiImpl.Search(new RemoteTaskQueueSearchRequest
                {
                    EnqueueDateTimeRange = Range.OfDate(t0, t1),
                    Names = new[] {typeof(BetaTaskData).Name}
                });
            searchResults2.TotalCount.Should().Be(1);
            searchResults2.TaskMetas.Single().Name.Should().Be(typeof(BetaTaskData).Name);

            var searchResults3 = remoteTaskQueueApiImpl.Search(new RemoteTaskQueueSearchRequest
                {
                    EnqueueDateTimeRange = Range.OfDate(t0, t1),
                    Names = new[] {typeof(BetaTaskData).Name, typeof(DeltaTaskData).Name}
                });
            searchResults3.TotalCount.Should().Be(2);
            searchResults3.TaskMetas.Select(x => x.Name).ShouldAllBeEquivalentTo(new[]
                {
                    typeof(BetaTaskData).Name,
                    typeof(DeltaTaskData).Name
                });
        }

        [Test]
        public void TestByTaskState()
        {
            var t0 = DateTime.Now;
            QueueTasksAndWaitForActualization(
                new FailingTaskData(),
                new AlphaTaskData());
            var t1 = DateTime.Now;
            var searchResults1 = remoteTaskQueueApiImpl.Search(new RemoteTaskQueueSearchRequest
                {
                    EnqueueDateTimeRange = Range.OfDate(t0, t1),
                    States = new[] {TaskState.Finished}
                });
            searchResults1.TotalCount.Should().Be(1);
            searchResults1.TaskMetas.Single().Name.Should().Be(typeof(AlphaTaskData).Name);

            var searchResults2 = remoteTaskQueueApiImpl.Search(new RemoteTaskQueueSearchRequest
                {
                    EnqueueDateTimeRange = Range.OfDate(t0, t1),
                    States = new[] {TaskState.Fatal}
                });
            searchResults2.TotalCount.Should().Be(1);
            searchResults2.TaskMetas.Single().Name.Should().Be(typeof(FailingTaskData).Name);
        }

        [Test]
        public void TestRerunTask()
        {
            var taskId = QueueTasksAndWaitForActualization(new AlphaTaskData()).Single();
            remoteTaskQueueApiImpl.GetTaskDetails(taskId).TaskMeta.Attempts.Should().Be(1);
            remoteTaskQueueApiImpl.RerunTasks(new[] {taskId});
            WaitForTasks(new[] {taskId}, TimeSpan.FromSeconds(60));
            remoteTaskQueueApiImpl.GetTaskDetails(taskId).TaskMeta.Attempts.Should().Be(2);
        }
        
        [Test]
        public void TestRerunTasksBySearchQuery()
        {
            var t0 = DateTime.Now;
            var taskId = QueueTasksAndWaitForActualization(
                new AlphaTaskData(),
                new AlphaTaskData(),
                new BetaTaskData());
            var t1 = DateTime.Now;

            remoteTaskQueueApiImpl.GetTaskDetails(taskId[0]).TaskMeta.Attempts.Should().Be(1);
            remoteTaskQueueApiImpl.GetTaskDetails(taskId[1]).TaskMeta.Attempts.Should().Be(1);
            remoteTaskQueueApiImpl.GetTaskDetails(taskId[2]).TaskMeta.Attempts.Should().Be(1);

            remoteTaskQueueApiImpl.RerunTasksBySearchQuery(new RemoteTaskQueueSearchRequest
                {
                    EnqueueDateTimeRange = Range.OfDate(t0, t1),
                    Names = new[] {typeof(AlphaTaskData).Name}
                });

            WaitForTasks(new[] {taskId[0], taskId[1]}, TimeSpan.FromSeconds(60));

            remoteTaskQueueApiImpl.GetTaskDetails(taskId[0]).TaskMeta.Attempts.Should().Be(2);
            remoteTaskQueueApiImpl.GetTaskDetails(taskId[1]).TaskMeta.Attempts.Should().Be(2);
            remoteTaskQueueApiImpl.GetTaskDetails(taskId[2]).TaskMeta.Attempts.Should().Be(1);            
        }

        private string[] QueueTasksAndWaitForActualization(params ITaskData[] taskDatas)
        {
            var taskIds = new List<string>();
            foreach(var taskData in taskDatas)
            {
                var task = remoteTaskQueue.CreateTask(taskData);
                task.Queue(TimeSpan.Zero);
                taskIds.Add(task.Id);
            }
            WaitForTasks(taskIds.ToArray(), TimeSpan.FromSeconds(60));
            elasticMonitoringServiceClient.UpdateAndFlush();
            return taskIds.ToArray();
        }

        private void WaitForTasks(IEnumerable<string> taskIds, TimeSpan timeSpan)
        {
            TaskSearchHelpers.WaitFor(() =>
                {
                    var tasks = remoteTaskQueue.HandleTaskCollection.GetTasks(taskIds.ToArray());
                    return tasks.All(t => t.Meta.State == TaskState.Finished || t.Meta.State == TaskState.Fatal);
                }, timeSpan);
        }

        [Injected]
        private IRemoteTaskQueueApiImpl remoteTaskQueueApiImpl;

        [Injected]
        private readonly IElasticMonitoringServiceClient elasticMonitoringServiceClient;

        [Injected]
        private readonly TaskSearchIndexSchema taskSearchIndexSchema;

        [Injected]
        private readonly RemoteQueue.Handling.RemoteTaskQueue remoteTaskQueue;
    }
}