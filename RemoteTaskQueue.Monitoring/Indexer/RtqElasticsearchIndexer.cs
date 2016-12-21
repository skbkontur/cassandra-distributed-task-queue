using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using MoreLinq;

using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using RemoteTaskQueue.Monitoring.Storage.Utils;
using RemoteTaskQueue.Monitoring.Storage.Writing;

using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexer : IRtqElasticsearchIndexer
    {
        public RtqElasticsearchIndexer(IGlobalTime globalTime,
                                       IEventLogRepository eventLogRepository,
                                       IHandleTasksMetaStorage handleTasksMetaStorage,
                                       ITaskMetaProcessor taskMetaProcessor,
                                       IRtqElasticsearchIndexerGraphiteReporter graphiteReporter,
                                       IRtqElasticsearchIndexerProgressMarkerStorage indexerProgressMarkerStorage)
        {
            this.globalTime = globalTime;
            this.eventLogRepository = eventLogRepository;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.taskMetaProcessor = taskMetaProcessor;
            this.graphiteReporter = graphiteReporter;
            this.indexerProgressMarkerStorage = indexerProgressMarkerStorage;
        }

        public void ProcessNewEvents()
        {
            var indexingStartTimestamp = indexerProgressMarkerStorage.GetIndexingStartTimestamp();
            var globalNowTimestamp = new Timestamp(globalTime.GetNowTicks());
            var indexingFinishTimestamp = indexerProgressMarkerStorage.IndexingFinishTimestamp ?? globalNowTimestamp;
            if(indexingStartTimestamp >= indexingFinishTimestamp)
            {
                Log.For(this).LogInfoFormat(string.Format("IndexingFinishTimestamp is reached: {0}", indexingFinishTimestamp));
                return;
            }
            Log.For(this).LogInfoFormat("Processing events from {0} to {1}", indexingStartTimestamp, indexingFinishTimestamp);

            var eventsToProcess = eventLogRepository.GetEvents(indexingStartTimestamp.Ticks - eventLogRepository.UnstableZoneLength.Ticks, indexingFinishTimestamp.Ticks, eventsReadingBatchSize)
                                                    .TakeWhile(x => x.Ticks <= indexingFinishTimestamp.Ticks);

            Timestamp lastEventTimestamp = null;
            Timestamp lastEventsBatchStartTimestamp = null;
            var taskIdsToProcess = new HashSet<string>();
            foreach(var @event in eventsToProcess)
            {
                taskIdsToProcess.Add(@event.TaskId);
                lastEventTimestamp = new Timestamp(@event.Ticks);
                if(lastEventsBatchStartTimestamp == null)
                    lastEventsBatchStartTimestamp = lastEventTimestamp;
                if(lastEventTimestamp - lastEventsBatchStartTimestamp > TimeSpan.FromHours(6) || taskIdsToProcess.Count > 10 * 1000 * 1000)
                {
                    ProcessTasks(taskIdsToProcess);
                    taskIdsToProcess.Clear();
                    lastEventsBatchStartTimestamp = null;
                    indexerProgressMarkerStorage.SetIndexingStartTimestamp(lastEventTimestamp);
                }
            }

            if(taskIdsToProcess.Any())
                ProcessTasks(taskIdsToProcess);

            indexerProgressMarkerStorage.SetIndexingStartTimestamp(lastEventTimestamp ?? indexingFinishTimestamp);
        }

        private void ProcessTasks([NotNull] HashSet<string> taskIdsToProcess)
        {
            taskIdsToProcess.Batch(taskIdsProcessingBatchSize, Enumerable.ToArray).AsParallel().WithDegreeOfParallelism(8).WithExecutionMode(ParallelExecutionMode.ForceParallelism).ForEach(taskIds =>
                {
                    var taskMetas = graphiteReporter.ReportTiming("ReadTaskMetas", () => handleTasksMetaStorage.GetMetas(taskIds));
                    var taskMetasToIndex = taskMetas.Values.Where(x => x.Ticks > indexerProgressMarkerStorage.InitialIndexingStartTimestamp.Ticks).ToArray();
                    taskMetaProcessor.IndexMetas(taskMetasToIndex);
                });
        }

        [NotNull]
        public RtqElasticsearchIndexerStatus GetStatus()
        {
            var lastIndexingStartTimestamp = indexerProgressMarkerStorage.GetIndexingStartTimestamp();
            return new RtqElasticsearchIndexerStatus
                {
                    ActualizationLag = Timestamp.Now - lastIndexingStartTimestamp,
                    IndexingFinishTimestamp = indexerProgressMarkerStorage.IndexingFinishTimestamp,
                    LastIndexingStartTimestamp = lastIndexingStartTimestamp,
                };
        }

        private const int eventsReadingBatchSize = 5000;
        private const int taskIdsProcessingBatchSize = 1000;
        private readonly IGlobalTime globalTime;
        private readonly IEventLogRepository eventLogRepository;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskMetaProcessor taskMetaProcessor;
        private readonly IRtqElasticsearchIndexerGraphiteReporter graphiteReporter;
        private readonly IRtqElasticsearchIndexerProgressMarkerStorage indexerProgressMarkerStorage;
    }
}