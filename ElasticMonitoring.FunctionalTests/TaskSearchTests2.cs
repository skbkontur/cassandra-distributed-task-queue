using System;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Client;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Client;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Search;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

using TestCommon;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    [EdiTestSuite("ElasticMonitoringTestSuite"), WithColumnFamilies, WithExchangeServices, WithApplicationSettings(FileName = "elasticMonitoringTests.csf")]
    public class TaskSearchTests2
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
        public void TestCloseNotExisting()
        {
            CloseAndCheck(DateTime.UtcNow.AddDays(-4), DateTime.UtcNow.AddDays(-2));
        }

        [Test]
        public void TestCloseAlreadyClosed()
        {
            var @from = DateTime.UtcNow.AddDays(-4);
            var to = DateTime.UtcNow.AddDays(-2);
            CloseAndCheck(@from, to);
            CheckIndicesClosed(@from, to);
            CloseAndCheck(@from, to);
        }

        [Test]
        public void TestCloseIndedAndSearchInOldIndex()
        {
            var client = elasticsearchClientFactory.GetClient();
            for(var i = 0; i < 5; i++)
            {
                var dt = DateTime.UtcNow.Subtract(TimeSpan.FromDays(i));
                var indexName = WriteIndexNameFactory.BuildIndexNameForTime(dt.Ticks, taskWriteDynamicSettings.CurrentIndexNameFormat);
                client.IndicesExists(indexName).ProcessResponse(404);
                client.IndicesCreate(indexName, new {});
            }
            var x1 = DateTime.UtcNow.AddDays(-1);
            var meta1 = CreateMeta(x1, false);
            taskWriter.IndexBatch(new[] {new Tuple<TaskMetaInformation, TaskExceptionInfo[], object>(meta1, new TaskExceptionInfo[0], new SlowTaskData {TimeMs = 1})});

            elasticMonitoringServiceClient.UpdateAndFlush();
            CheckSearch(string.Format("Data.TimeMs:1"), x1, x1, meta1.Id);

            Assert.AreEqual(0, client.Search(taskWriteDynamicSettings.OldDataIndex, new {}).ProcessResponse().Response["hits"]["total"]);

            CloseAndCheck(DateTime.UtcNow.AddDays(-4), DateTime.UtcNow.AddDays(-2));

            var x2 = DateTime.UtcNow.AddDays(-2);
            var meta2 = CreateMeta(x2, true);

            var x3 = DateTime.UtcNow;
            var meta3 = CreateMeta(x3, false);
            
            CheckSearch(string.Format("Data.TimeMs:123"), x2, x2, new string[0]); //note alias redirected to old data but it is empty now

            taskWriter.IndexBatch(new[]
                {
                    new Tuple<TaskMetaInformation, TaskExceptionInfo[], object>(meta2, new TaskExceptionInfo[0], new SlowTaskData {TimeMs = 2}), //old data
                    new Tuple<TaskMetaInformation, TaskExceptionInfo[], object>(meta3, new TaskExceptionInfo[0], new SlowTaskData {TimeMs = 3}), //current data
                });

            elasticMonitoringServiceClient.UpdateAndFlush();

            CheckSearch(string.Format("Data.TimeMs:2"), x2, x2, meta2.Id);

            CheckSearch(string.Format("Data.TimeMs:3"), x3, x3, meta3.Id);

            Assert.AreEqual(1, client.Search(taskWriteDynamicSettings.OldDataIndex, new {}).ProcessResponse().Response["hits"]["total"]);
        }

        private void CloseAndCheck(DateTime @from, DateTime to)
        {
            service.CloseOldIndices(@from, to);
            CheckIndicesClosed(@from, to);
        }

        private void CheckIndicesClosed(DateTime @from, DateTime to)
        {
            var indexForTimeRange = searchIndexNameFactory.GetIndexForTimeRange(@from.Ticks, to.Ticks, taskWriteDynamicSettings.CurrentIndexNameFormat);
            var client = elasticsearchClientFactory.GetClient();
            var response = client.ClusterState("metadata", indexForTimeRange).ProcessResponse();
            foreach(var obj in response.Response["metadata"]["indices"])
                Assert.AreEqual("close", obj.Value.state.ToString(), string.Format("index '{0}' not closed", obj.Name));
        }

        private void CheckSearch(string q, DateTime from, DateTime to, params string[] ids)
        {
            TaskSearchHelpers.CheckSearch(taskSearchClient, q, from, to, ids);
        }

        private static TaskMetaInformation CreateMeta(DateTime ticks, bool isOld)
        {
            var ticks2 = ticks.ToUniversalTime().Ticks;
            var lastModificationTicks = isOld ? ticks.ToUniversalTime().AddDays(1.1).Ticks : ticks2;
            Assert.That(lastModificationTicks <= DateTime.UtcNow.Ticks);
            return new TaskMetaInformation("SlowTaskData", Guid.NewGuid().ToString()) {Ticks = ticks2, LastModificationTicks = lastModificationTicks};
        }

        // ReSharper disable UnassignedReadonlyField.Compiler
        [Injected]
        private readonly SearchIndexNameFactory searchIndexNameFactory;

        [Injected]
        private readonly InternalDataElasticsearchFactory elasticsearchClientFactory;

        [Injected]
        private readonly TaskIndexCloseService service;

        [Injected]
        private readonly IElasticMonitoringServiceClient elasticMonitoringServiceClient;

        [Injected]
        private readonly TaskSearchIndexSchema taskSearchIndexSchema;

        [Injected]
        private readonly ITaskSearchClient taskSearchClient;

        [Injected]
        private readonly ITaskWriteDynamicSettings taskWriteDynamicSettings;

        [Injected]
        private readonly TaskWriter taskWriter;

        // ReSharper restore UnassignedReadonlyField.Compiler
    }
}