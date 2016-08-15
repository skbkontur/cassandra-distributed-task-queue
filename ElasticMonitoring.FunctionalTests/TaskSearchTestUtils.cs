using System;
using System.Linq;

using GroboContainer.Core;

using NUnit.Framework;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Configuration;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing;

using TestCommon;
using TestCommon.NUnitWrappers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    [EdiTestSuite, WithApplicationSettings(FileName = "elasticMonitoringTests.csf"),
     WithDefaultSerializer,
     WithCassandra("CatalogueCluster", "QueueKeyspace"),
     WithRemoteLock("remoteLock")]
    public class TaskSearchTestUtils
    {
        [Test, Ignore]
        public void TestDeleteRemoteLock()
        {
            cassandraCluster.RetrieveColumnFamilyConnection("QueueKeyspace", "remoteLock").Truncate();
            cassandraCluster.RetrieveColumnFamilyConnection("QueueKeyspace", RemoteTaskQueueLockConstants.LockColumnFamily).Truncate();
        }

        [Test, Ignore]
        public void TestCreateTaskSearchSchema()
        {
            hackService.DeleteAll();
            taskSearchIndexSchema.ActualizeTemplate();
        }

        [Test, Ignore]
        public void TestDeleteElasticDataAndSchema()
        {
            hackService.DeleteAll();
        }

        [Test, Ignore]
        public void TestDeleteAllIndices()
        {
            var client = elasticsearchClientFactory.GetClient();
            client.ClearScroll("_all").ProcessResponse();
            client.IndicesDelete("_all").ProcessResponse();
        }

        [Test, Ignore]
        public void TestCreateSomeOldIndices()
        {
            taskSearchIndexSchema.ActualizeTemplate();
            var client = elasticsearchClientFactory.GetClient();
            for(var i = 0; i < 10; i++)
            {
                var dt = DateTime.UtcNow.Subtract(TimeSpan.FromDays(i));
                var indexName = WriteIndexNameFactory.BuildIndexNameForTime(dt.Ticks, IndexNameConverter.ConvertToDateTimeFormat("monitoring-index-{yyyy.MM.dd}")); //todo get from settings??
                if(client.IndicesExists(indexName).ProcessResponse(200, 404).HttpStatusCode == 404)
                {
                    Console.WriteLine("CREATING: " + indexName);
                    client.IndicesCreate(indexName, new {});
                }
            }
        }

        [Test, Ignore]
        public void TestCreateCassandraSchema()
        {
            container.DropAndCreateDatabase(columnFamilyRegistry.GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily
                        {
                            Name = "columnFamilyName",
                        },
                    new ColumnFamily
                        {
                            Name = "remoteLock",
                        }
                }).ToArray());
        }

        // ReSharper disable UnassignedReadonlyField.Compiler
        [Injected]
        private readonly IColumnFamilyRegistry columnFamilyRegistry;

        [Injected]
        private readonly IContainer container;

        [Injected]
        private readonly ICassandraCluster cassandraCluster;

        [Injected]
        private readonly TaskSearchIndexSchema taskSearchIndexSchema;

        [Injected]
        private readonly TaskSearchIndexDataTestService hackService;

        [Injected]
        private readonly IElasticsearchClientFactory elasticsearchClientFactory;

        // ReSharper restore UnassignedReadonlyField.Compiler
    }
}