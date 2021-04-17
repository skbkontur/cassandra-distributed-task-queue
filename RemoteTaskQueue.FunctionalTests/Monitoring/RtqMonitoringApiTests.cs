using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace RemoteTaskQueue.FunctionalTests.Monitoring
{
    public class RtqMonitoringApiTests : MonitoringTestBase
    {
        [Test]
        public void TestPaging()
        {
            var t0 = Timestamp.Now;
            var taskIds = QueueTasksAndWaitForActualization(
                Enumerable.Range(0, 20)
                          .Select(x => new AlphaTaskData()).Cast<IRtqTaskData>().ToArray());
            var t1 = Timestamp.Now;
            var searchResults1 = rtqMonitoringApi.Search(new RtqMonitoringSearchRequest
                {
                    EnqueueTimestampRange = EnqueueTimestampRange(t0, t1),
                    Offset = 0,
                    Count = 10,
                });
            searchResults1.TotalCount.Should().Be(20);
            var searchResults2 = rtqMonitoringApi.Search(new RtqMonitoringSearchRequest
                {
                    EnqueueTimestampRange = EnqueueTimestampRange(t0, t1),
                    Offset = 10,
                    Count = 10,
                });
            searchResults2.TotalCount.Should().Be(20);
            searchResults1.TaskMetas.Select(x => x.Id).Concat(searchResults2.TaskMetas.Select(x => x.Id)).Should().BeEquivalentTo(taskIds);
        }

        [Test]
        public void TestSearchByRange()
        {
            var t0 = Timestamp.Now;
            var taskIds = QueueTasksAndWaitForActualization(
                Enumerable.Range(0, 20)
                          .Select(x => new AlphaTaskData()).Cast<IRtqTaskData>().ToArray());
            var t1 = Timestamp.Now;
            var searchResults = rtqMonitoringApi.Search(new RtqMonitoringSearchRequest
                {
                    EnqueueTimestampRange = EnqueueTimestampRange(t0, t1),
                    Offset = 0,
                    Count = 20,
                });
            searchResults.TotalCount.Should().Be(20);
            searchResults.TaskMetas.Select(x => x.Id).Should().BeEquivalentTo(taskIds);
        }

        [Test]
        public void TestByTaskNames()
        {
            var t0 = Timestamp.Now;
            QueueTasksAndWaitForActualization(
                new AlphaTaskData(),
                new BetaTaskData(),
                new DeltaTaskData());
            var t1 = Timestamp.Now;
            var searchResults1 = rtqMonitoringApi.Search(new RtqMonitoringSearchRequest
                {
                    EnqueueTimestampRange = EnqueueTimestampRange(t0, t1),
                    Names = new[] {typeof(AlphaTaskData).Name},
                    Offset = 0,
                    Count = 20,
                });
            searchResults1.TotalCount.Should().Be(1);
            searchResults1.TaskMetas.Single().Name.Should().Be(typeof(AlphaTaskData).Name);

            var searchResults2 = rtqMonitoringApi.Search(new RtqMonitoringSearchRequest
                {
                    EnqueueTimestampRange = EnqueueTimestampRange(t0, t1),
                    Names = new[] {typeof(BetaTaskData).Name},
                    Offset = 0,
                    Count = 20,
                });
            searchResults2.TotalCount.Should().Be(1);
            searchResults2.TaskMetas.Single().Name.Should().Be(typeof(BetaTaskData).Name);

            var searchResults3 = rtqMonitoringApi.Search(new RtqMonitoringSearchRequest
                {
                    EnqueueTimestampRange = EnqueueTimestampRange(t0, t1),
                    Names = new[] {typeof(BetaTaskData).Name, typeof(DeltaTaskData).Name},
                    Offset = 0,
                    Count = 20,
                });
            searchResults3.TotalCount.Should().Be(2);
            searchResults3.TaskMetas.Select(x => x.Name).Should().BeEquivalentTo(new[]
                {
                    typeof(BetaTaskData).Name,
                    typeof(DeltaTaskData).Name
                });
        }

        [Test]
        public void TestByTaskState()
        {
            var t0 = Timestamp.Now;
            QueueTasksAndWaitForActualization(
                new FailingTaskData(),
                new AlphaTaskData());
            var t1 = Timestamp.Now;
            var searchResults1 = rtqMonitoringApi.Search(new RtqMonitoringSearchRequest
                {
                    EnqueueTimestampRange = EnqueueTimestampRange(t0, t1),
                    States = new[] {TaskState.Finished},
                    Offset = 0,
                    Count = 20,
                });
            searchResults1.TotalCount.Should().Be(1);
            searchResults1.TaskMetas.Single().Name.Should().Be(typeof(AlphaTaskData).Name);

            var searchResults2 = rtqMonitoringApi.Search(new RtqMonitoringSearchRequest
                {
                    EnqueueTimestampRange = EnqueueTimestampRange(t0, t1),
                    States = new[] {TaskState.Fatal},
                    Offset = 0,
                    Count = 20,
                });
            searchResults2.TotalCount.Should().Be(1);
            searchResults2.TaskMetas.Single().Name.Should().Be(typeof(FailingTaskData).Name);
        }

        [Test]
        public void TestByTaskStateAndTaskName()
        {
            var t0 = Timestamp.Now;
            QueueTasksAndWaitForActualization(
                new FailingTaskData(),
                new AlphaTaskData());
            var t1 = Timestamp.Now;
            var searchResults1 = rtqMonitoringApi.Search(new RtqMonitoringSearchRequest
                {
                    EnqueueTimestampRange = EnqueueTimestampRange(t0, t1),
                    States = new[] {TaskState.Finished},
                    Names = new[] {typeof(AlphaTaskData).Name},
                    Offset = 0,
                    Count = 20,
                });
            searchResults1.TotalCount.Should().Be(1);
            searchResults1.TaskMetas.Single().Name.Should().Be(typeof(AlphaTaskData).Name);

            var searchResults2 = rtqMonitoringApi.Search(new RtqMonitoringSearchRequest
                {
                    EnqueueTimestampRange = EnqueueTimestampRange(t0, t1),
                    States = new[] {TaskState.Fatal},
                    Names = new[] {typeof(AlphaTaskData).Name},
                    Offset = 0,
                    Count = 20,
                });
            searchResults2.TotalCount.Should().Be(0);
            searchResults2.TaskMetas.Should().BeEmpty();

            var searchResults3 = rtqMonitoringApi.Search(new RtqMonitoringSearchRequest
                {
                    EnqueueTimestampRange = EnqueueTimestampRange(t0, t1),
                    States = new[] {TaskState.Finished},
                    Names = new[] {typeof(FailingTaskData).Name},
                    Offset = 0,
                    Count = 20,
                });
            searchResults3.TotalCount.Should().Be(0);
            searchResults3.TaskMetas.Should().BeEmpty();
        }

        [Test]
        public void TestRerunTask()
        {
            var taskId = QueueTasksAndWaitForActualization(new AlphaTaskData()).Single();
            rtqMonitoringApi.GetTaskDetails(taskId).TaskMeta.Attempts.Should().Be(1);
            rtqMonitoringApi.RerunTasks(new[] {taskId});
            WaitForTasks(new[] {taskId}, TimeSpan.FromSeconds(60));
            rtqMonitoringApi.GetTaskDetails(taskId).TaskMeta.Attempts.Should().Be(2);
        }

        [Test]
        public void TestRerunTasksBySearchQuery()
        {
            var t0 = Timestamp.Now;
            var taskId = QueueTasksAndWaitForActualization(
                new AlphaTaskData(),
                new AlphaTaskData(),
                new BetaTaskData());
            var t1 = Timestamp.Now;

            rtqMonitoringApi.GetTaskDetails(taskId[0]).TaskMeta.Attempts.Should().Be(1);
            rtqMonitoringApi.GetTaskDetails(taskId[1]).TaskMeta.Attempts.Should().Be(1);
            rtqMonitoringApi.GetTaskDetails(taskId[2]).TaskMeta.Attempts.Should().Be(1);

            rtqMonitoringApi.RerunTasksBySearchQuery(new RtqMonitoringSearchRequest
                {
                    EnqueueTimestampRange = EnqueueTimestampRange(t0, t1),
                    Names = new[] {typeof(AlphaTaskData).Name},
                });

            WaitForTasks(new[] {taskId[0], taskId[1]}, TimeSpan.FromSeconds(60));

            rtqMonitoringApi.GetTaskDetails(taskId[0]).TaskMeta.Attempts.Should().Be(2);
            rtqMonitoringApi.GetTaskDetails(taskId[1]).TaskMeta.Attempts.Should().Be(2);
            rtqMonitoringApi.GetTaskDetails(taskId[2]).TaskMeta.Attempts.Should().Be(1);
        }

        private static TimestampRange EnqueueTimestampRange(Timestamp t0, Timestamp t1)
        {
            return new TimestampRange {LowerBound = t0, UpperBound = t1};
        }

        private string[] QueueTasksAndWaitForActualization(params IRtqTaskData[] taskDatas)
        {
            var taskIds = new List<string>();
            foreach (var taskData in taskDatas)
            {
                var task = remoteTaskQueue.CreateTask(taskData);
                task.Queue(TimeSpan.Zero);
                taskIds.Add(task.Id);
            }
            WaitForTasks(taskIds.ToArray(), TimeSpan.FromSeconds(60));
            monitoringServiceClient.ExecuteForcedFeeding();
            return taskIds.ToArray();
        }

        private void WaitForTasks(IEnumerable<string> taskIds, TimeSpan timeSpan)
        {
            Assert.That(() =>
                            {
                                var tasks = remoteTaskQueue.HandleTaskCollection.GetTasks(taskIds.ToArray());
                                return tasks.All(t => t.Meta.State == TaskState.Finished || t.Meta.State == TaskState.Fatal);
                            },
                        Is.True.After((int)timeSpan.TotalMilliseconds, 100));
        }

        [Injected]
        private RtqMonitoringApi rtqMonitoringApi;
    }
}