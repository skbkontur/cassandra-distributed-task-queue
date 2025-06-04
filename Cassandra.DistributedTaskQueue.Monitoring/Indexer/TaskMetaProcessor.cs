#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Elasticsearch.Net;

using GroBuf;

using MoreLinqInlined;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Utils;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer;

public class TaskMetaProcessor
{
    public TaskMetaProcessor(ILog logger,
                             RtqElasticsearchIndexerSettings settings,
                             IRtqElasticsearchClient elasticClient,
                             RemoteTaskQueue remoteTaskQueue,
                             RtqMonitoringPerfGraphiteReporter perfGraphiteReporter)
    {
        this.logger = logger.ForContext(nameof(TaskMetaProcessor));
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

    public void ProcessTasks(List<string> taskIdsToProcess)
    {
        logger.Info("Processing tasks: {ProcessingTasksCount}", new {ProcessingTasksCount = taskIdsToProcess.Count});
        taskIdsToProcess.Batch(settings.TaskIdsProcessingBatchSize, Enumerable.ToArray)
                        .AsParallel()
                        .WithDegreeOfParallelism(settings.IndexingThreadsCount)
                        .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                        .ForAll(taskIds =>
                            {
                                var taskMetas = perfGraphiteReporter.ReportTiming("ReadTaskMetas", () => handleTasksMetaStorage.GetMetas(taskIds));
                                var taskMetasToIndex = taskMetas.Values.Where(x => x.Ticks > settings.InitialIndexingStartTimestamp.Ticks).ToArray();
                                if (taskMetasToIndex.Any())
                                    IndexMetas(taskMetasToIndex);
                            });
    }

    private void IndexMetas(TaskMetaInformation[] batch)
    {
        var taskDatas = perfGraphiteReporter.ReportTiming("ReadTaskDatas", () => taskDataStorage.Read(batch));
        var taskExceptionInfos = perfGraphiteReporter.ReportTiming("ReadTaskExceptionInfos", () => taskExceptionInfoStorage.Read(batch));
        var enrichedBatch = new ( TaskMetaInformation TaskMeta, TaskExceptionInfo[] TaskExceptionInfos, object? TaskData)[batch.Length];
        for (var i = 0; i < batch.Length; i++)
        {
            var taskMeta = batch[i];
            object? taskDataObj = null;
            if (taskDatas.TryGetValue(taskMeta.Id, out var taskData))
            {
                if (taskDataRegistry.TryGetTaskType(taskMeta.Name, out var taskType))
                    taskDataObj = TryDeserializeTaskData(taskType!, taskData, taskMeta);
            }
            enrichedBatch[i] = (taskMeta, taskExceptionInfos[taskMeta.Id], taskDataObj);
        }
        perfGraphiteReporter.ReportTiming("IndexBatch", () => IndexBatch(enrichedBatch));
    }

    private void IndexBatch((TaskMetaInformation TaskMeta, TaskExceptionInfo[] TaskExceptionInfos, object? TaskData)[] batch)
    {
        logger.Info("IndexBatch: {BatchLength} tasks", new {BatchLength = batch.Length});
        var payload = new string[batch.Length * 2];
        for (var i = 0; i < batch.Length; i++)
        {
            payload[2 * i] = JsonSerializer.Serialize(new {index = CreateIndexInfo(batch[i].TaskMeta)});
            var taskIndexedInfo = BuildTaskIndexedInfo(batch[i].TaskMeta, batch[i].TaskExceptionInfos, batch[i].TaskData);
            payload[2 * i + 1] = JsonSerializer.Serialize(taskIndexedInfo, RtqElasticsearchIndexerSettings.GetJsonOptions());
        }
        perfGraphiteReporter.ReportTiming("ElasticsearchClient_Bulk", () => elasticClient.Bulk<StringResponse>(PostData.MultiJson(payload), bulkRequestTimeout).DieIfBulkRequestFailed());
    }

    private object CreateIndexInfo(TaskMetaInformation taskMeta)
    {
        var indexName = DateTimeFormatter.DateFromTicks(taskMeta.Ticks).ToString(RtqElasticsearchConsts.DataIndexNameFormat);
        if (elasticClient.UseElastic7)
        {
            return new
                {
                    _index = indexName,
                    _id = taskMeta.Id,
                };
        }
        return new
            {
                _index = indexName,
                _type = RtqElasticsearchConsts.RtqIndexTypeName, // see https://www.elastic.co/guide/en/elasticsearch/reference/current/removal-of-types.html
                _id = taskMeta.Id,
            };
    }

    private object TryDeserializeTaskData(Type taskType, byte[] taskData, TaskMetaInformation taskMetaInformation)
    {
        try
        {
            return serializer.Deserialize(taskType, taskData);
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to deserialize taskData for: {RtqTaskMeta}", new {RtqTaskMeta = taskMetaInformation});
            return null;
        }
    }

    private static object BuildTaskIndexedInfo(TaskMetaInformation taskMeta, TaskExceptionInfo[] taskExceptionInfos, object? taskData)
    {
        var executionDurationTicks = taskMeta.ExecutionDurationTicks;
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
                LastExecutionDurationInMs = executionDurationTicks != null ? TimeSpan.FromTicks(executionDurationTicks.Value).TotalMilliseconds : (double?)null
            };
        var exceptionInfo = string.Join("\r\n", taskExceptionInfos.Reverse().Select(x => x.ExceptionMessageInfo));
        return new TaskIndexedInfo(meta, exceptionInfo, taskData);
    }

    private readonly ILog logger;
    private readonly RtqElasticsearchIndexerSettings settings;
    private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
    private readonly IRtqTaskDataRegistry taskDataRegistry;
    private readonly ITaskDataStorage taskDataStorage;
    private readonly ITaskExceptionInfoStorage taskExceptionInfoStorage;
    private readonly ISerializer serializer;
    private readonly RtqMonitoringPerfGraphiteReporter perfGraphiteReporter;
    private readonly IRtqElasticsearchClient elasticClient;
    private readonly BulkRequestParameters bulkRequestTimeout;
}