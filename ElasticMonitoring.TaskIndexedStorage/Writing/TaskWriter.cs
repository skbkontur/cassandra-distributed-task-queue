using Elasticsearch.Net;

using JetBrains.Annotations;

using log4net;

using Newtonsoft.Json;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Bulk;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing.Contracts;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    public class TaskWriter
    {
        public TaskWriter(
            IElasticsearchClientFactory elasticsearchClientFactory,
            IWriteIndexNameFactory indexNameFactory,
            TaskDataService taskDataService)
        {
            this.indexNameFactory = indexNameFactory;
            this.taskDataService = taskDataService;
            elasticsearchClient = elasticsearchClientFactory.GetClient(new JsonSerializerSettings
                {
                    ContractResolver = new OmitNonIndexablePropertiesContractResolver(),
                    Converters = new JsonConverter[]
                        {
                            new LongStringsToNullConverter(500)
                        }
                });
        }

        public void IndexBatch(TaskMetaInformation[] metas, TaskExceptionInfo[] exceptionInfos, object[] taskDatas)
        {
            logger.LogInfoFormat("IndexBatch: {0} tasks", metas.Length);
            IndexTasks(metas, exceptionInfos, taskDatas);
        }

        private void IndexTasks(TaskMetaInformation[] metas, TaskExceptionInfo[] exceptionInfos, object[] taskDatas)
        {
            var body = new object[metas.Length * 2];
            for(var i = 0; i < metas.Length; i++)
            {
                var meta = metas[i];
                var taskData = taskDatas[i];
                var indexName = indexNameFactory.GetIndexForTask(meta);
                body[2 * i] = new
                    {
                        index = new
                            {
                                _index = indexName,
                                _type = meta.Name,
                                _id = meta.Id
                            }
                    };

                body[2 * i + 1] = BuildSavedData(meta, exceptionInfos[i], taskData);
            }
            elasticsearchClient.Bulk<BulkResponse>(body).DieIfErros();
        }

        private object BuildSavedData([NotNull] TaskMetaInformation meta, [CanBeNull] TaskExceptionInfo exceptionInfo, object taskData)
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
                    StartExecutingTime = meta.StartExecutingTicks,
                    Exception = exceptionInfo == null ? null : exceptionInfo.ExceptionMessageInfo,
                };
            return taskDataService.CreateTaskIndexedInfo(metaIndexedInfo, taskData);
        }

        private readonly IWriteIndexNameFactory indexNameFactory;
        private readonly TaskDataService taskDataService;

        private readonly IElasticsearchClient elasticsearchClient;

        private static readonly ILog logger = LogManager.GetLogger("TaskWriter");
    }
}