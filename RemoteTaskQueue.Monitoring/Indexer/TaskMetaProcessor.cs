using System;
using System.Collections.Generic;
using System.Linq;

using Elasticsearch.Net;

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
using SKBKontur.Catalogue.Objects.Json;

using Vostok.Logging.Abstractions;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class TaskMetaProcessor
    {
        public TaskMetaProcessor(ILog logger,
                                 RtqElasticsearchIndexerSettings settings,
                                 IRtqElasticsearchClient elasticClient,
                                 RemoteQueue.Handling.RemoteTaskQueue remoteTaskQueue,
                                 RtqMonitoringPerfGraphiteReporter perfGraphiteReporter)
        {
            this.logger = logger;
            this.settings = settings;
            handleTasksMetaStorage = remoteTaskQueue.HandleTasksMetaStorage;
            taskDataRegistry = remoteTaskQueue.TaskDataRegistry;
            taskDataStorage = remoteTaskQueue.TaskDataStorage;
            taskExceptionInfoStorage = remoteTaskQueue.TaskExceptionInfoStorage;
            serializer = remoteTaskQueue.Serializer;
            this.perfGraphiteReporter = perfGraphiteReporter;
            this.elasticClient = elasticClient;
            bulkRequestTimeout = new BulkRequestParameters
                {
                    Timeout = this.settings.BulkIndexRequestTimeout,
                    RequestConfiguration = new RequestConfiguration {RequestTimeout = this.settings.BulkIndexRequestTimeout}
                };
        }

        public void ProcessTasks([NotNull, ItemNotNull] List<string> taskIdsToProcess)
        {
            logger.Info(string.Format("Processing tasks: {0}", taskIdsToProcess.Count));
            taskIdsToProcess.Batch(settings.TaskIdsProcessingBatchSize, Enumerable.ToArray)
                            .AsParallel()
                            .WithDegreeOfParallelism(settings.IndexingThreadsCount)
                            .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                            .ForEach(taskIds =>
                                {
                                    var taskMetas = perfGraphiteReporter.ReportTiming("ReadTaskMetas", () => handleTasksMetaStorage.GetMetas(taskIds));
                                    var taskMetasToIndex = taskMetas.Values.Where(x => x.Ticks > settings.InitialIndexingStartTimestamp.Ticks).ToArray();
                                    if (taskMetasToIndex.Any())
                                        IndexMetas(taskMetasToIndex);
                                });
        }

        private void IndexMetas([NotNull, ItemNotNull] TaskMetaInformation[] batch)
        {
            var taskDatas = perfGraphiteReporter.ReportTiming("ReadTaskDatas", () => taskDataStorage.Read(batch));
            var taskExceptionInfos = perfGraphiteReporter.ReportTiming("ReadTaskExceptionInfos", () => taskExceptionInfoStorage.Read(batch));
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
            perfGraphiteReporter.ReportTiming("IndexBatch", () => IndexBatch(enrichedBatch));
        }

        private void IndexBatch([NotNull] ( /*[NotNull]*/ TaskMetaInformation TaskMeta, /*[NotNull, ItemNotNull]*/ TaskExceptionInfo[] TaskExceptionInfos, /*[CanBeNull]*/ object TaskData)[] batch)
        {
            logger.Info(string.Format("IndexBatch: {0} tasks", batch.Length));
            var payload = new string[batch.Length * 2];
            for (var i = 0; i < batch.Length; i++)
            {
                payload[2 * i] = new
                    {
                        index = new
                            {
                                _index = DateTimeFormatter.DateFromTicks(batch[i].TaskMeta.Ticks).ToString(RtqElasticsearchConsts.DataIndexNameFormat),
                                _type = "doc_type_is_deprecated", // see https://www.elastic.co/guide/en/elasticsearch/reference/current/removal-of-types.html
                                _id = batch[i].TaskMeta.Id
                            }
                    }.ToJson();
                payload[2 * i + 1] = BuildTaskIndexedInfo(batch[i].TaskMeta, batch[i].TaskExceptionInfos, batch[i].TaskData).ToJson(settings.JsonSerializerSettings);
            }
            perfGraphiteReporter.ReportTiming("ElasticsearchClient_Bulk", () => elasticClient.Bulk<StringResponse>(PostData.MultiJson(payload), bulkRequestTimeout).DieIfBulkRequestFailed());
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
                logger.Error(e, string.Format("Failed to deserialize taskData for: {0}", taskMetaInformation));
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

        private readonly ILog logger;
        private readonly RtqElasticsearchIndexerSettings settings;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskDataRegistry taskDataRegistry;
        private readonly ITaskDataStorage taskDataStorage;
        private readonly ITaskExceptionInfoStorage taskExceptionInfoStorage;
        private readonly ISerializer serializer;
        private readonly RtqMonitoringPerfGraphiteReporter perfGraphiteReporter;
        private readonly IRtqElasticsearchClient elasticClient;
        private readonly BulkRequestParameters bulkRequestTimeout;
    }
}