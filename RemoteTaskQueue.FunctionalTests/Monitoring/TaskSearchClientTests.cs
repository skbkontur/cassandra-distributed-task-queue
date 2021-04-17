using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using FluentAssertions;

using GroboContainer.NUnitExtensions;

using GroBuf;

using NUnit.Framework;

using RemoteTaskQueue.FunctionalTests.Common;
using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Client;
using SkbKontur.Cassandra.TimeBasedUuid;

using Vostok.Logging.Abstractions;

namespace RemoteTaskQueue.FunctionalTests.Monitoring
{
    public class TaskSearchClientTests : MonitoringTestBase
    {
        [Test]
        public void TestCreateNotDeserializedTaskData()
        {
            var t0 = Timestamp.Now;
            var taskId = QueueTask(new SlowTaskData(), TimeSpan.FromSeconds(1));
            Log.For(this).Info("TaskId: {TaskId}", new {TaskId = taskId});
            var taskMetaInformation = handleTasksMetaStorage.GetMeta(taskId);

            var badBytes = new byte[] {};
            Assert.Catch<Exception>(() => serializer.Deserialize<SlowTaskData>(badBytes));

            taskDataStorage.Overwrite(taskMetaInformation, badBytes);

            var taskId2 = QueueTask(new SlowTaskData(), TimeSpan.FromSeconds(2));

            WaitForTasks(new[] {taskId, taskId2}, TimeSpan.FromSeconds(5));

            var t1 = Timestamp.Now;

            monitoringServiceClient.ExecuteForcedFeeding();

            CheckSearch($"Meta.Id:\"{taskId}\"", t0, t1, taskId);
            CheckSearch($"Meta.Id:\"{taskId2}\"", t0, t1, taskId2);
        }

        [Test]
        public void TestStupid()
        {
            var t0 = Timestamp.Now;

            var taskId = QueueTask(new SlowTaskData());
            WaitForTasks(new[] {taskId}, TimeSpan.FromSeconds(5));

            monitoringServiceClient.ExecuteForcedFeeding();

            var t1 = Timestamp.Now;
            CheckSearch("*", t0, t1, taskId);
            CheckSearch($"\"{taskId}\"", t0, t1, taskId);
            CheckSearch($"Meta.Id:\"{taskId}\"", t0, t1, taskId);
            CheckSearch($"Meta.Name:{typeof(SlowTaskData).Name}", t0, t1, taskId);
            CheckSearch("Meta.Name:Zzz", t0, t1);
        }

        [Test]
        public void TestSearchByExceptionSubstring()
        {
            var t0 = Timestamp.Now;

            var uniqueData = Guid.NewGuid();
            Log.For(this).Info("ud={UniqueData}", new {UniqueData = uniqueData});
            var taskId = QueueTask(new FailingTaskData {UniqueData = uniqueData, RetryCount = 0});
            WaitForTasks(new[] {taskId}, TimeSpan.FromSeconds(5));

            monitoringServiceClient.ExecuteForcedFeeding();

            var t1 = Timestamp.Now;
            CheckSearch($"\"{taskId}\"", t0, t1, taskId);
            CheckSearch($"\"{uniqueData}\"", t0, t1, taskId);
            CheckSearch($"ExceptionInfo:\"{uniqueData}\"", t0, t1, taskId);
            CheckSearch($"\"{Guid.NewGuid()}\"", t0, t1);
        }

        [Test]
        public void TestSearchByExpiration()
        {
            var t0 = Timestamp.Now;

            var taskId = QueueTask(new SlowTaskData());
            WaitForTasks(new[] {taskId}, TimeSpan.FromSeconds(5));

            monitoringServiceClient.ExecuteForcedFeeding();

            var t1 = Timestamp.Now;
            var ttl = TestRtqSettings.StandardTestTaskTtl;
            Log.For(this).Info("{IsoTime}", new {IsoTime = ToIsoTime(new Timestamp(remoteTaskQueue.GetTaskInfo<SlowTaskData>(taskId).Context.ExpirationTimestampTicks.Value))});
            CheckSearch($"Meta.ExpirationTime: [\"{ToIsoTime(t0 + ttl)}\" TO \"{ToIsoTime(t1 + ttl + TimeSpan.FromSeconds(10))}\"]", t0, t1, taskId);
        }

        private static string ToIsoTime(Timestamp timestamp)
        {
            return timestamp.ToDateTime().ToString("O");
        }

        [Test]
        public void TestSearchByExceptionSubstring_MultipleDifferentErrors()
        {
            var t0 = Timestamp.Now;

            var uniqueData = Guid.NewGuid();
            Log.For(this).Info("ud={UniqueData}", new {UniqueData = uniqueData});
            var failingTaskData = new FailingTaskData {UniqueData = uniqueData, RetryCount = 2};
            var taskId = QueueTask(failingTaskData);
            WaitForTasks(new[] {taskId}, TimeSpan.FromSeconds(30));

            monitoringServiceClient.ExecuteForcedFeeding();

            var t1 = Timestamp.Now;
            CheckSearch($"\"{taskId}\"", t0, t1, taskId);
            CheckSearch($"\"{uniqueData}\"", t0, t1, taskId);
            CheckSearch($"\"{Guid.NewGuid()}\"", t0, t1);

            for (var attempts = 1; attempts <= 3; attempts++)
                CheckSearch($"\"FailingTask failed: {failingTaskData}. Attempts = {attempts}\"", t0, t1, taskId);
        }

        [Test]
        public void TestUpdateAndFlush()
        {
            var t0 = Timestamp.Now;
            for (var i = 0; i < 100; i++)
            {
                Log.For(this).Info("Iteration: {Iteration}", new {Iteration = i});
                var taskId0 = QueueTask(new SlowTaskData());
                WaitForTasks(new[] {taskId0}, TimeSpan.FromSeconds(5));
                monitoringServiceClient.ExecuteForcedFeeding();
                CheckSearch($"\"{taskId0}\"", t0, Timestamp.Now, taskId0);
            }
        }

        [Test]
        public void TestDataSearchBug()
        {
            var t0 = Timestamp.Now;
            for (var i = 0; i < 100; i++)
            {
                Log.For(this).Info("Iteration: {Iteration}", new {Iteration = i});
                var taskId0 = QueueTask(new SlowTaskData());
                var taskId1 = QueueTask(new SlowTaskData());
                var taskId2 = QueueTask(new SlowTaskData());
                WaitForTasks(new[] {taskId0, taskId1, taskId2}, TimeSpan.FromSeconds(5));
                monitoringServiceClient.ExecuteForcedFeeding();
                CheckSearch($"\"{taskId0}\"", t0, Timestamp.Now, taskId0);
            }
        }

        [Test]
        public void TestNotStupid()
        {
            var t0 = Timestamp.Now;
            var taskId0 = QueueTask(new SlowTaskData());
            WaitForTasks(new[] {taskId0}, TimeSpan.FromSeconds(5));

            var t1 = Timestamp.Now;

            var taskId1 = QueueTask(new AlphaTaskData());
            WaitForTasks(new[] {taskId1}, TimeSpan.FromSeconds(5));

            monitoringServiceClient.ExecuteForcedFeeding();

            var t2 = Timestamp.Now;

            CheckSearch("*", t0, t2, taskId0, taskId1);

            CheckSearch($"Meta.Id:\"{taskId0}\" OR Meta.Id:\"{taskId1}\"", t0, t1, taskId0, taskId1);
            CheckSearch("Meta.State:Finished", t0, t1, taskId0, taskId1);

            CheckSearch("NOT _exists_:Meta.ParentTaskId", t0, t1, taskId0, taskId1);
            CheckSearch("NOT _exists_:Meta.Name", t0, t1);

            CheckSearch("Data.SlowTaskData.TimeMs:0", t0, t1, taskId0);
            CheckSearch("Data.SlowTaskData.UseCounter:false", t0, t1, taskId0);
        }

        [Test]
        public void TestTaskDatasWithCommonFieldName()
        {
            var t0 = Timestamp.Now;

            var taskId0 = QueueTask(new AlphaTaskData {FieldWithCommonName = 42});
            WaitForTasks(new[] {taskId0}, TimeSpan.FromSeconds(5));
            monitoringServiceClient.ExecuteForcedFeeding();

            var taskId1 = QueueTask(new GammaTaskData {FieldWithCommonName = "abc xyz"});
            WaitForTasks(new[] {taskId1}, TimeSpan.FromSeconds(5));
            monitoringServiceClient.ExecuteForcedFeeding();

            var taskId2 = QueueTask(new DeltaTaskData {FieldWithCommonName = new[] {47}});
            WaitForTasks(new[] {taskId2}, TimeSpan.FromSeconds(5));
            monitoringServiceClient.ExecuteForcedFeeding();

            var t1 = Timestamp.Now;

            CheckSearch("*", t0, t1, taskId0, taskId1, taskId2);
            CheckSearch($"Data.AlphaTaskData.FieldWithCommonName:42", t0, t1, taskId0);
            CheckSearch($"Data.GammaTaskData.FieldWithCommonName:\"abc xyz\"", t0, t1, taskId1);
            CheckSearch($"Data.DeltaTaskData.FieldWithCommonName:47", t0, t1, taskId2);
            CheckSearch($"abc", t0, t1, taskId1);
            CheckSearch($"xyz", t0, t1, taskId1);
        }

        [Test]
        public void TestFieldsWithAbstractTypesAreIgnored()
        {
            var t0 = Timestamp.Now;

            var taskId0 = QueueTask(new AlphaTaskData
                {
                    NonIndexableField = new StructuredTaskDataDetails
                        {
                            Info = new StructuredTaskDataDetails.DetailsInfo
                                {
                                    SomeInfo = "abc"
                                }
                        }
                });
            WaitForTasks(new[] {taskId0}, TimeSpan.FromSeconds(5));
            monitoringServiceClient.ExecuteForcedFeeding();

            var taskId1 = QueueTask(new AlphaTaskData
                {
                    NonIndexableField = new PlainTaskDataDetails
                        {
                            Info = "def"
                        }
                });
            WaitForTasks(new[] {taskId1}, TimeSpan.FromSeconds(5));
            monitoringServiceClient.ExecuteForcedFeeding();

            var taskId2 = QueueTask(new GammaTaskData
                {
                    NonIndexableField = new StructuredTaskDataDetails
                        {
                            Info = new StructuredTaskDataDetails.DetailsInfo
                                {
                                    SomeInfo = "ghi"
                                }
                        }
                });
            WaitForTasks(new[] {taskId2}, TimeSpan.FromSeconds(5));
            monitoringServiceClient.ExecuteForcedFeeding();

            var taskId3 = QueueTask(new GammaTaskData
                {
                    NonIndexableField = new PlainTaskDataDetails
                        {
                            Info = "jkl"
                        }
                });
            WaitForTasks(new[] {taskId3}, TimeSpan.FromSeconds(5));
            monitoringServiceClient.ExecuteForcedFeeding();

            var taskId4 = QueueTask(new DeltaTaskData
                {
                    NonIndexableField = new StructuredTaskDataDetails
                        {
                            Info = new StructuredTaskDataDetails.DetailsInfo
                                {
                                    SomeInfo = "mno"
                                }
                        }
                });
            WaitForTasks(new[] {taskId4}, TimeSpan.FromSeconds(5));
            monitoringServiceClient.ExecuteForcedFeeding();

            var taskId5 = QueueTask(new DeltaTaskData
                {
                    NonIndexableField = new PlainTaskDataDetails
                        {
                            Info = "pqr"
                        }
                });
            WaitForTasks(new[] {taskId5}, TimeSpan.FromSeconds(5));
            monitoringServiceClient.ExecuteForcedFeeding();

            var t1 = Timestamp.Now;

            CheckSearch("*", t0, t1, taskId0, taskId1, taskId2, taskId3, taskId4, taskId5);
            CheckSearch($"abc", t0, t1);
            CheckSearch($"def", t0, t1);
            CheckSearch($"ghi", t0, t1);
            CheckSearch($"jkl", t0, t1);
            CheckSearch($"mno", t0, t1);
            CheckSearch($"pqr", t0, t1);
        }

        [Test]
        public void TestTaskChain()
        {
            var t0 = Timestamp.Now;
            var chainId = Guid.NewGuid();
            var taskId0 = QueueTask(new AlphaTaskData {ChainId = chainId});
            var taskId1 = QueueTask(new GammaTaskData {ChainId = chainId});
            var taskId2 = QueueTask(new DeltaTaskData {ChainId = Guid.NewGuid()});
            var taskId3 = QueueTask(new DeltaTaskData {ChainId = chainId});
            var taskId4 = QueueTask(new AlphaTaskData {ChainId = Guid.NewGuid()});
            var taskId5 = QueueTask(new GammaTaskData {ChainId = Guid.NewGuid()});
            WaitForTasks(new[] {taskId0, taskId1, taskId2, taskId3, taskId4, taskId5}, TimeSpan.FromSeconds(5));
            monitoringServiceClient.ExecuteForcedFeeding();

            var t1 = Timestamp.Now;
            CheckSearch("*", t0, t1, taskId0, taskId1, taskId2, taskId3, taskId4, taskId5);
            CheckSearch($"\"{chainId}\"", t0, t1, taskId0, taskId1, taskId3);
            CheckSearch($"Data.AlphaTaskData.ChainId:\"{chainId}\" OR Data.GammaTaskData.ChainId:\"{chainId}\" OR Data.DeltaTaskData.ChainId:\"{chainId}\"", t0, t1, taskId0, taskId1, taskId3);
            CheckSearch($"Data.\\*.ChainId:\"{chainId}\"", t0, t1, taskId0, taskId1, taskId3);
        }

        [Test]
        public void TestTaskWithEnumSearch()
        {
            var t0 = Timestamp.Now;
            var taskId = QueueTask(new EpsilonTaskData(EpsilonEnum.Delta));
            WaitForTasks(new[] {taskId}, TimeSpan.FromSeconds(5));
            monitoringServiceClient.ExecuteForcedFeeding();

            var t1 = Timestamp.Now;
            CheckSearch("Data.\\*.EpsilonEnum:\"Delta\"", t0, t1, taskId);
            CheckSearch("Data.\\*.EpsilonEnum:\"Epsilon\"", t0, t1);
        }

        [Test]
        public void TestPaging()
        {
            var t0 = Timestamp.Now;
            var taskIds = new List<string>();
            const int pageSize = 5;
            for (var i = 0; i < pageSize + pageSize / 2; i++)
            {
                taskIds.Add(QueueTask(new AlphaTaskData()));
                Thread.Sleep(TimeSpan.FromMilliseconds(1)); // note: elastic stores timestamps with millisecond precision
            }
            taskIds.Reverse();
            var expectedTaskIds = taskIds.ToArray();
            WaitForTasks(expectedTaskIds, TimeSpan.FromSeconds(60));
            monitoringServiceClient.ExecuteForcedFeeding();
            var t1 = Timestamp.Now;

            var resultsPage1 = taskSearchClient.Search(new TaskSearchRequest
                                                           {
                                                               FromTicksUtc = t0.Ticks,
                                                               ToTicksUtc = t1.Ticks,
                                                               QueryString = "*",
                                                           },
                                                       0,
                                                       pageSize);
            resultsPage1.TotalCount.Should().Be(expectedTaskIds.Length);
            resultsPage1.Ids.Should().Equal(expectedTaskIds.Take(pageSize).ToArray());

            var resultsPage2 = taskSearchClient.Search(new TaskSearchRequest
                                                           {
                                                               FromTicksUtc = t0.Ticks,
                                                               ToTicksUtc = t1.Ticks,
                                                               QueryString = "*",
                                                           },
                                                       pageSize,
                                                       pageSize);
            resultsPage2.TotalCount.Should().Be(expectedTaskIds.Length);
            resultsPage2.Ids.Should().Equal(expectedTaskIds.Skip(pageSize).ToArray());
        }

        private string QueueTask<T>(T taskData, TimeSpan? delay = null) where T : IRtqTaskData
        {
            var task = remoteTaskQueue.CreateTask(taskData);
            task.Queue(delay ?? TimeSpan.Zero);
            return task.Id;
        }

        private void WaitForTasks(IEnumerable<string> taskIds, TimeSpan timeout)
        {
            Assert.That(() =>
                            {
                                var tasks = remoteTaskQueue.HandleTaskCollection.GetTasks(taskIds.ToArray());
                                return tasks.All(t => t.Meta.State == TaskState.Finished || t.Meta.State == TaskState.Fatal);
                            },
                        Is.True.After((int)timeout.TotalMilliseconds, 100));
        }

        [Test]
        public void UpdateSchemaMultipleTimes()
        {
            Assert.DoesNotThrow(() => schema.Actualize(local : true, bulkLoad : false));
            Assert.DoesNotThrow(() => schema.Actualize(local : true, bulkLoad : false));
        }

        [Test]
        public void SearchByTimeGuid()
        {
            var t0 = Timestamp.Now;
            var timeGuid0 = TimeGuid.NowGuid();
            var taskId0 = QueueTask(new TimeGuidTaskData {Value = timeGuid0});
            var timeGuid1 = TimeGuid.NowGuid();
            var taskId1 = QueueTask(new TimeGuidTaskData {Value = timeGuid1});
            WaitForTasks(new[] {taskId0, taskId1}, TimeSpan.FromSeconds(5));
            monitoringServiceClient.ExecuteForcedFeeding();

            var t1 = Timestamp.Now;
            CheckSearch("*", t0, t1, taskId0, taskId1);
            CheckSearch($"Data.\\*.Value:\"{timeGuid0.ToGuid().ToString()}\"", t0, t1, taskId0);
            CheckSearch($"Data.\\*.Value:\"{timeGuid1.ToGuid().ToString()}\"", t0, t1, taskId1);
        }

        [Test]
        public void TestFilterExecutionTime()
        {
            var t0 = Timestamp.Now;
            var taskId0 = QueueTask(new SlowTaskData {TimeMs = 100});
            var taskId1 = QueueTask(new SlowTaskData {TimeMs = 1000});
            WaitForTasks(new[] {taskId0, taskId1}, TimeSpan.FromSeconds(5));
            monitoringServiceClient.ExecuteForcedFeeding();

            var t1 = Timestamp.Now;
            CheckSearch("*", t0, t1, taskId0, taskId1);
            CheckSearch($"Meta.LastExecutionDurationInMs:[0 TO 500]", t0, t1, taskId0);
            CheckSearch($"Meta.LastExecutionDurationInMs:[500 TO 2000]", t0, t1, taskId1);
            CheckSearch($"Meta.LastExecutionDurationInMs:[0 TO 450.666]", t0, t1, taskId0);
            CheckSearch($"Meta.LastExecutionDurationInMs:[550.666 TO 2000]", t0, t1, taskId1);
            CheckSearch($"Meta.LastExecutionDurationInMs:[* TO 450.666]", t0, t1, taskId0);
            CheckSearch($"Meta.LastExecutionDurationInMs:[550.666 TO *]", t0, t1, taskId1);
            CheckSearch($"Meta.LastExecutionDurationInMs:[* TO *]", t0, t1, taskId0, taskId1);
        }

        [Injected]
        private readonly RtqElasticsearchSchema schema;

        [Injected]
        private ISerializer serializer;

        [Injected]
        private HandleTasksMetaStorage handleTasksMetaStorage;

        [Injected]
        private TaskDataStorage taskDataStorage;
    }
}