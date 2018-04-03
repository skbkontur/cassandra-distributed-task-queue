using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Configuration;

using RemoteTaskQueue.Monitoring.Storage.Utils;
using RemoteTaskQueue.Monitoring.Storage.Writing;

using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class TaskMetaProcessor
    {
        public TaskMetaProcessor(RtqElasticsearchIndexerSettings settings,
                                 IHandleTasksMetaStorage handleTasksMetaStorage,
                                 ITaskDataRegistry taskDataRegistry,
                                 ITaskDataStorage taskDataStorage,
                                 ITaskExceptionInfoStorage taskExceptionInfoStorage,
                                 TaskWriter writer,
                                 ISerializer serializer,
                                 IRtqElasticsearchIndexerGraphiteReporter graphiteReporter)
        {
            this.settings = settings;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.taskDataRegistry = taskDataRegistry;
            this.taskDataStorage = taskDataStorage;
            this.taskExceptionInfoStorage = taskExceptionInfoStorage;
            this.writer = writer;
            this.serializer = serializer;
            this.graphiteReporter = graphiteReporter;
        }

        public void ProcessTasks([NotNull] List<string> taskIdsToProcess)
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

        private void IndexMetas([NotNull] TaskMetaInformation[] batch)
        {
            var taskDatas = graphiteReporter.ReportTiming("ReadTaskDatas", () => taskDataStorage.Read(batch));
            var taskExceptionInfos = graphiteReporter.ReportTiming("ReadTaskExceptionInfos", () => taskExceptionInfoStorage.Read(batch));
            var enrichedBatch = new Tuple<TaskMetaInformation, TaskExceptionInfo[], object>[batch.Length];
            for (var i = 0; i < batch.Length; i++)
            {
                var taskMeta = batch[i];
                byte[] taskData;
                object taskDataObj = null;
                if (taskDatas.TryGetValue(taskMeta.Id, out taskData))
                {
                    Type taskType;
                    if (taskDataRegistry.TryGetTaskType(taskMeta.Name, out taskType))
                        taskDataObj = DeserializeTaskDataSafe(taskType, taskData, taskMeta);
                }
                enrichedBatch[i] = Tuple.Create(taskMeta, taskExceptionInfos[taskMeta.Id], taskDataObj);
            }
            graphiteReporter.ReportTiming("IndexBatch", () => writer.IndexBatch(enrichedBatch));
        }

        private object DeserializeTaskDataSafe(Type taskType, byte[] taskData, TaskMetaInformation taskMetaInformation)
        {
            try
            {
                return serializer.Deserialize(taskType, taskData);
            }
            catch (Exception e)
            {
                Log.For(this).LogErrorFormat(e, "Data deserialization error. Taskmeta {0}", taskMetaInformation);
                return null;
            }
        }

        private readonly RtqElasticsearchIndexerSettings settings;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskDataRegistry taskDataRegistry;
        private readonly ITaskDataStorage taskDataStorage;
        private readonly ITaskExceptionInfoStorage taskExceptionInfoStorage;
        private readonly TaskWriter writer;
        private readonly ISerializer serializer;
        private readonly IRtqElasticsearchIndexerGraphiteReporter graphiteReporter;
    }
}