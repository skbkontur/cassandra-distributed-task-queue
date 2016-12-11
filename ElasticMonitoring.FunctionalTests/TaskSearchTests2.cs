using System;

using Elasticsearch.Net;

using FluentAssertions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;

using RemoteTaskQueue.Monitoring.Indexer;
using RemoteTaskQueue.Monitoring.Storage;
using RemoteTaskQueue.Monitoring.Storage.Search;
using RemoteTaskQueue.Monitoring.Storage.Utils;
using RemoteTaskQueue.Monitoring.Storage.Writing;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

#pragma warning disable 649

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    public class TaskSearchTests2 : SearchTasksTestBase
    {
        [Test]
        public void TestCloseNotExisting()
        {
            CloseAndCheck(Timestamp.Now.AddDays(-4), Timestamp.Now.AddDays(-2));
        }

        [Test]
        public void TestCloseAlreadyClosed()
        {
            var from = Timestamp.Now.AddDays(-4);
            var to = Timestamp.Now.AddDays(-2);
            CloseAndCheck(from, to);
            CheckIndicesClosed(from, to);
            CloseAndCheck(from, to);
        }

        [Test]
        public void TestCloseAlreadyClosedWithoutAliases()
        {
            var from = Timestamp.Now.AddDays(-4);
            var to = Timestamp.Now.AddDays(-4);
            CloseByHand(from, to);
            CheckIndicesClosed(from, to);

            CheckIndexAlias(SearchIndexNameFactory.GetIndexForTimeRange(from.Ticks, from.Ticks, WriteIndexNameFactory.CurrentIndexNameFormat), new[]
                {
                    GetOldDataAlias(from),
                    GetSearchAlias(from),
                }, true);
            CheckIndexAlias(RtqElasticsearchConsts.OldDataIndex, new[]
                {
                    GetOldDataAlias(from),
                    GetSearchAlias(from),
                }, false);

            CloseAndCheck(Timestamp.Now.AddDays(-4), Timestamp.Now.AddDays(-2));
            //NOTE check aliases moved correctly
            CheckIndexAlias(SearchIndexNameFactory.GetIndexForTimeRange(from.Ticks, from.Ticks, WriteIndexNameFactory.CurrentIndexNameFormat), new[]
                {
                    GetOldDataAlias(from),
                    GetSearchAlias(from),
                }, false);
            CheckIndexAlias(RtqElasticsearchConsts.OldDataIndex, new[]
                {
                    GetOldDataAlias(from),
                    GetSearchAlias(from),
                }, true);
        }

        [Test]
        public void TestCloseIndexAndSearchInOldIndex()
        {
            var client = elasticsearchClientFactory.DefaultClient.Value;
            for(var i = 0; i < 5; i++)
            {
                var dt = Timestamp.Now.Subtract(TimeSpan.FromDays(i));
                var indexName = WriteIndexNameFactory.BuildIndexNameForTime(dt.Ticks, WriteIndexNameFactory.CurrentIndexNameFormat);
                client.IndicesExists(indexName).ProcessResponse(404);
                client.IndicesCreate(indexName, new {}).ProcessResponse();
            }
            var x1 = Timestamp.Now.AddDays(-1);
            var meta1 = CreateMeta(x1, false);
            taskWriter.IndexBatch(new[] {new Tuple<TaskMetaInformation, TaskExceptionInfo[], object>(meta1, new TaskExceptionInfo[0], new SlowTaskData {TimeMs = 1})});

            elasticMonitoringServiceClient.UpdateAndFlush();
            CheckSearch("Data.TimeMs:1", x1, x1, meta1.Id);

            Assert.AreEqual(0, client.Search(RtqElasticsearchConsts.OldDataIndex, new {}).ProcessResponse().Response["hits"]["total"]);

            CloseAndCheck(Timestamp.Now.AddDays(-4), Timestamp.Now.AddDays(-2));

            var x2 = Timestamp.Now.AddDays(-2);
            var meta2 = CreateMeta(x2, true);

            var x3 = Timestamp.Now;
            var meta3 = CreateMeta(x3, false);

            CheckSearch("Data.TimeMs:123", x2, x2); //note alias redirected to old data but it is empty now

            taskWriter.IndexBatch(new[]
                {
                    new Tuple<TaskMetaInformation, TaskExceptionInfo[], object>(meta2, new TaskExceptionInfo[0], new SlowTaskData {TimeMs = 2}), //old data
                    new Tuple<TaskMetaInformation, TaskExceptionInfo[], object>(meta3, new TaskExceptionInfo[0], new SlowTaskData {TimeMs = 3}), //current data
                });

            elasticMonitoringServiceClient.UpdateAndFlush();

            CheckSearch("Data.TimeMs:2", x2, x2, meta2.Id);

            CheckSearch("Data.TimeMs:3", x3, x3, meta3.Id);

            Assert.AreEqual(1, client.Search(RtqElasticsearchConsts.OldDataIndex, new {}).ProcessResponse().Response["hits"]["total"]);
        }

        private void CloseAndCheck(Timestamp from, Timestamp to)
        {
            taskIndexCloseService.CloseOldIndices(from, to);
            CheckIndicesClosed(from, to);
        }

        private static string GetOldDataAlias(Timestamp timestamp)
        {
            return IndexNameConverter.FillIndexNamePlaceholder(RtqElasticsearchConsts.OldDataAliasFormat, SearchIndexNameFactory.GetIndexForTimeRange(timestamp.Ticks, timestamp.Ticks, WriteIndexNameFactory.CurrentIndexNameFormat));
        }

        private static string GetSearchAlias(Timestamp timestamp)
        {
            return SearchIndexNameFactory.GetIndexForTimeRange(timestamp.Ticks, timestamp.Ticks);
        }

        private void CheckIndicesClosed(Timestamp from, Timestamp to)
        {
            var indexForTimeRange = SearchIndexNameFactory.GetIndexForTimeRange(from.Ticks, to.Ticks, WriteIndexNameFactory.CurrentIndexNameFormat);
            var client = elasticsearchClientFactory.DefaultClient.Value;
            var response = client.ClusterState("metadata", indexForTimeRange).ProcessResponse();
            foreach(var obj in response.Response["metadata"]["indices"])
                Assert.AreEqual("close", obj.Value.state.ToString(), string.Format("index '{0}' not closed", obj.Name));
        }

        private static TaskMetaInformation CreateMeta(Timestamp timestamp, bool isOld)
        {
            var lastModificationTicks = isOld ? timestamp.AddDays(1.1).Ticks : timestamp.Ticks;
            Assert.That(lastModificationTicks <= Timestamp.Now.Ticks);
            return new TaskMetaInformation("SlowTaskData", Guid.NewGuid().ToString()) {Ticks = timestamp.Ticks, LastModificationTicks = lastModificationTicks};
        }

        private void CheckIndexAlias(string index, string[] expected, bool shouldInclude)
        {
            var client = elasticsearchClientFactory.DefaultClient.Value;
            var response = client.ClusterState("metadata", index).ProcessResponse();
            foreach(var obj in response.Response["metadata"]["indices"])
            {
                var aliases = ((JArray)obj.Value.aliases).ToObject<string[]>();
                if(shouldInclude)
                    expected.Should().BeSubsetOf(aliases);
                else
                    expected.Should().NotBeSubsetOf(aliases);
            }
        }

        private void CloseByHand(Timestamp from, Timestamp to)
        {
            var indexForTimeRange = SearchIndexNameFactory.GetIndexForTimeRange(from.Ticks, to.Ticks, WriteIndexNameFactory.CurrentIndexNameFormat);

            var elasticsearchClient = elasticsearchClientFactory.DefaultClient.Value;
            foreach(var index in indexForTimeRange.Split(','))
            {
                var response = elasticsearchClient.CatIndices(index, p => p.H("status")).ProcessResponse(200, 404);
                if(response.HttpStatusCode == 404)
                {
                    var respCreate = elasticsearchClient.IndicesCreate(index, new {}).ProcessResponse(200, 400);
                    if(!(respCreate.HttpStatusCode == 400 && respCreate.ServerError != null && respCreate.ServerError.ExceptionType == "IndexAlreadyExistsException"))
                        respCreate.ProcessResponse(); //note throw any other error
                }
                elasticsearchClient.ClusterHealth(index, p => p.WaitForStatus(WaitForStatus.Green)).ProcessResponse();
                elasticsearchClient.IndicesClose(index).ProcessResponse();
            }
        }

        [Injected]
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;

        [Injected]
        private readonly TaskIndexCloseService taskIndexCloseService;

        [Injected]
        private readonly TaskWriter taskWriter;
    }
}