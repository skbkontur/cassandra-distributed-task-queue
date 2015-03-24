using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.NUnit.Extensions.TestEnvironments.Cassandra;
using SKBKontur.Catalogue.NUnit.Extensions.TestEnvironments.Container;
using SKBKontur.Catalogue.NUnit.Extensions.TestEnvironments.PropertyInjection;
using SKBKontur.Catalogue.NUnit.Extensions.TestEnvironments.Serializer;
using SKBKontur.Catalogue.NUnit.Extensions.TestEnvironments.Settings;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Client;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests.Environment;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    [ContainerEnvironment, Cassandra, DefaultSettings, DefaultSerializer, InjectProperties, RemoteTaskQueueRemoteLock, TestExchangeServices]
    public class TaskMonitoringIndexTests
    {
        [Test]
        public void TestCounter()
        {
            var taskData = new SlowTaskData() {TimeMs = 1000};
            var remoteTask = RemoteTaskQueue.CreateTask(taskData);
            remoteTask.Queue();
            WaitForTasks(new[] {remoteTask.Id}, TimeSpan.FromMinutes(15));

            ElasticMonitoringServiceClient.UpdateAndFlush();


        }

        private void WaitFor(Func<bool> func, TimeSpan timeout, int checkTimeout = 99)
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
            WaitFor(() => taskIds.All(taskId => RemoteTaskQueue.GetTaskInfo<SlowTaskData>(taskId).Context.State == TaskState.Finished), timeSpan);
        }

        [Injected]
        public IElasticMonitoringServiceClient ElasticMonitoringServiceClient { get; set; }

        [Injected]
        public IRemoteTaskQueue RemoteTaskQueue { get; set; }

        [Injected]
        public IEventLogRepository EventLogRepository { get; set; }
    }
}