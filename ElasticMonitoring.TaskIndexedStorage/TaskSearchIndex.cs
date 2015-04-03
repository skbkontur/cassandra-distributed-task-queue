using Elasticsearch.Net;

using log4net;

using Newtonsoft.Json;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Bulk;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage
{
    public class TaskSearchIndex
    {
        public TaskSearchIndex(
            IElasticsearchClientFactory elasticsearchClientFactory)
        {
            elasticsearchClient = elasticsearchClientFactory.GetClient(new JsonSerializerSettings
                {
                    ContractResolver = new ContractResolverWithOmitedByteArrays()
                });
        }

        public void Refresh()
        {
            elasticsearchClient.IndicesRefresh("_all");
        }

        public void IndexBatch(TaskMetaInformation[] metas, object[] taskDatas)
        {
            logger.InfoFormat("IndexBatch: {0} tasks", metas.Length);
            IndexTasks(metas, taskDatas);
        }

        private void IndexTasks(TaskMetaInformation[] metas, object[] taskDatas)
        {
            var body = new object[metas.Length * 2];
            for(var i = 0; i < metas.Length; i++)
            {
                var meta = metas[i];
                var taskData = taskDatas[i];
                var taskCreatedTime = meta.Ticks; //NOTE these time newer changed (for concrete task)
                var indexName = IndexNameFactory.GetIndexForTime(taskCreatedTime);
                body[2 * i] = new
                    {
                        index = new
                            {
                                _index = indexName,
                                _type = meta.Name,
                                _id = meta.Id
                            }
                    };

                body[2 * i + 1] = BuildSavedData(meta, taskData);
            }
            elasticsearchClient.Bulk<BulkResponse>(body).DieIfErros();
        }

        private static object BuildSavedData(TaskMetaInformation meta, object taskData)
        {
            var metaIndexedInfo = new MetaIndexedInfo
                {
                    Id = meta.Id,
                    Name = meta.Name,
                    State = meta.State.ToString(),
                    Attempts = meta.Attempts,
                    ParentTaskId = meta.ParentTaskId,
                    TaskGroupLock = meta.TaskGroupLock,
                    EnqueueTime = meta.Ticks,
                    FinishExecutingTime = meta.FinishExecutingTicks,
                    LastModificationTime = meta.LastModificationTicks.Value, //todo hack
                    MinimalStartTime = meta.MinimalStartTicks,
                    StartExecutingTime = meta.StartExecutingTicks
                };
            return new {Meta = metaIndexedInfo, Data = taskData};
        }

        private readonly IElasticsearchClient elasticsearchClient;

        private static readonly ILog logger = LogManager.GetLogger("TaskSearchIndex");
    }
}