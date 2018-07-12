using System;
using System.Collections.Generic;
using System.Linq;

using Elasticsearch.Net;
using Elasticsearch.Net.Connection.Configuration;

using GroBuf;

using JetBrains.Annotations;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Configuration;

using RemoteTaskQueue.Monitoring.Storage;
using RemoteTaskQueue.Monitoring.Storage.Utils;
using RemoteTaskQueue.Monitoring.Storage.Writing;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Bulk;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class TaskMetaProcessor
    {
        public TaskMetaProcessor(RtqElasticsearchIndexerSettings settings,
                                 RtqElasticsearchClientFactory elasticsearchClientFactory,
                                 IHandleTasksMetaStorage handleTasksMetaStorage,
                                 ITaskDataRegistry taskDataRegistry,
                                 ITaskDataStorage taskDataStorage,
                                 ITaskExceptionInfoStorage taskExceptionInfoStorage,
                                 ISerializer serializer,
                                 IRtqElasticsearchIndexerGraphiteReporter graphiteReporter)
        {
            this.settings = settings;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.taskDataRegistry = taskDataRegistry;
            this.taskDataStorage = taskDataStorage;
            this.taskExceptionInfoStorage = taskExceptionInfoStorage;
            this.serializer = serializer;
            this.graphiteReporter = graphiteReporter;
            elasticsearchClient = elasticsearchClientFactory.CreateClient(settings.JsonSerializerSettings);
        }

        public void ProcessTasks([NotNull, ItemNotNull] List<string> taskIdsToProcess)
        {
            Log.For(this).LogInfoFormat("Processing tasks: {0}", taskIdsToProcess.Count);
            taskIdsToProcess.Batch(settings.TaskIdsProcessingBatchSize, Enumerable.ToArray)
                            .AsParallel()
                            .WithDegreeOfParallelism(settings.IndexingThreadsCount)
                            .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                            .ForEach(taskIds =>
                                {
                                    var taskMetas = graphiteReporter.ReportTiming("ReadTaskMetas", () => handleTasksMetaStorage.GetMetas(taskIds));
                                    var taskMetasToIndex = taskMetas.Values.Where(x => x.Ticks > settings.InitialIndexingStartTimestamp.Ticks).ToArray();
                                    if (taskMetasToIndex.Any())
                                        IndexMetas(taskMetasToIndex);
                                });
        }

        private void IndexMetas([NotNull, ItemNotNull] TaskMetaInformation[] batch)
        {
            var taskDatas = graphiteReporter.ReportTiming("ReadTaskDatas", () => taskDataStorage.Read(batch));
            var taskExceptionInfos = graphiteReporter.ReportTiming("ReadTaskExceptionInfos", () => taskExceptionInfoStorage.Read(batch));
            var enrichedBatch = new ( /*[NotNull]*/ TaskMetaInformation TaskMeta, /*[NotNull, ItemNotNull]*/ TaskExceptionInfo[] TaskExceptionInfos, /*[CanBeNull]*/ object TaskData)[batch.Length];
            for (var i = 0; i < batch.Length; i++)
            {
                var taskMeta = batch[i];
                object taskDataObj = null;
                if (taskDatas.TryGetValue(taskMeta.Id, out var taskData))
                {
                    if (taskDataRegistry.TryGetTaskType(taskMeta.Name, out var taskType))
                        taskDataObj = TryDeserializeTaskData(taskType, taskData, taskMeta);
                }
                enrichedBatch[i] = (taskMeta, taskExceptionInfos[taskMeta.Id], taskDataObj);
            }
            graphiteReporter.ReportTiming("IndexBatch", () => IndexBatch(enrichedBatch));
        }

        private void IndexBatch([NotNull] ( /*[NotNull]*/ TaskMetaInformation TaskMeta, /*[NotNull, ItemNotNull]*/ TaskExceptionInfo[] TaskExceptionInfos, /*[CanBeNull]*/ object TaskData)[] batch)
        {
            Log.For(this).LogInfoFormat("IndexBatch: {0} tasks", batch.Length);
            var body = new object[batch.Length * 2];
            for (var i = 0; i < batch.Length; i++)
            {
                body[2 * i] = new
                    {
                        index = new
                            {
                                _index = DateTimeFormatter.DateFromTicks(batch[i].TaskMeta.Ticks).ToString(RtqElasticsearchConsts.DataIndexNameFormat),
                                _type = "doc_type_is_deprecated", // see https://www.elastic.co/guide/en/elasticsearch/reference/current/removal-of-types.html
                                _id = batch[i].TaskMeta.Id
                            }
                    };
                body[2 * i + 1] = BuildTaskIndexedInfo(batch[i].TaskMeta, batch[i].TaskExceptionInfos, batch[i].TaskData);
            }
            graphiteReporter.ReportTiming("ElasticsearchClient_Bulk", () => elasticsearchClient.Bulk<BulkResponse>(body, SetRequestTimeout).DieIfErros());
        }

        [NotNull]
        private static BulkRequestParameters SetRequestTimeout([NotNull] BulkRequestParameters requestParameters)
        {
            return requestParameters.Timeout("5m").RequestConfiguration(x => new RequestConfigurationDescriptor().RequestTimeout((int)TimeSpan.FromMinutes(5).TotalMilliseconds));
        }

        [CanBeNull]
        private object TryDeserializeTaskData([NotNull] Type taskType, [NotNull] byte[] taskData, [NotNull] TaskMetaInformation taskMetaInformation)
        {
            try
            {
                return serializer.Deserialize(taskType, taskData);
            }
            catch (Exception e)
            {
                Log.For(this).LogErrorFormat(e, "Failed to deserialize taskData for: {0}", taskMetaInformation);
                return null;
            }
        }

        [NotNull]
        private static object BuildTaskIndexedInfo([NotNull] TaskMetaInformation taskMeta, [NotNull, ItemNotNull] TaskExceptionInfo[] taskExceptionInfos, [CanBeNull] object taskData)
        {
            var meta = new MetaIndexedInfo
                {
                    Id = taskMeta.Id,
                    Name = taskMeta.Name,
                    State = taskMeta.State.ToString(),
                    Attempts = taskMeta.Attempts,
                    ParentTaskId = taskMeta.ParentTaskId,
                    TaskGroupLock = taskMeta.TaskGroupLock,
                    EnqueueTime = taskMeta.Ticks,
                    FinishExecutingTime = taskMeta.FinishExecutingTicks,
                    LastModificationTime = taskMeta.LastModificationTicks ?? 0,
                    MinimalStartTime = taskMeta.MinimalStartTicks,
                    StartExecutingTime = taskMeta.StartExecutingTicks,
                    ExpirationTime = taskMeta.ExpirationTimestampTicks ?? 0,
                };
            var exceptionInfo = string.Join("\r\n", taskExceptionInfos.Reverse().Select(x => x.ExceptionMessageInfo));
            return new TaskIndexedInfo(meta, exceptionInfo, taskData);
        }

        private readonly RtqElasticsearchIndexerSettings settings;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskDataRegistry taskDataRegistry;
        private readonly ITaskDataStorage taskDataStorage;
        private readonly ITaskExceptionInfoStorage taskExceptionInfoStorage;
        private readonly ISerializer serializer;
        private readonly IRtqElasticsearchIndexerGraphiteReporter graphiteReporter;
        private readonly IElasticsearchClient elasticsearchClient;
    }
}