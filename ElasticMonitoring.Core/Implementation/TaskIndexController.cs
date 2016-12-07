using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.Objects.Json;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Types;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public class TaskIndexController : ITaskIndexController
    {
        public TaskIndexController(
            IEventLogRepository eventLogRepository,
            IMetaCachedReader reader,
            ITaskMetaProcessor taskMetaProcessor,
            IRtqElasticsearchIndexingProgressMarkerStorage indexingProgressMarkerStorage,
            IGlobalTime globalTime,
            IRtqElasticsearchIndexerGraphiteReporter graphiteReporter)
        {
            this.eventLogRepository = eventLogRepository;
            this.reader = reader;
            this.taskMetaProcessor = taskMetaProcessor;
            this.globalTime = globalTime;
            this.graphiteReporter = graphiteReporter;
            this.indexingProgressMarkerStorage = indexingProgressMarkerStorage;
            unstableZoneTicks = eventLogRepository.UnstableZoneLength.Ticks;
            unprocessedEventsMap = new EventsMap(unstableZoneTicks * 2);
            processedEventsMap = new EventsMap(unstableZoneTicks * 2);
            Interlocked.Exchange(ref lastTicks, unknownTicks);
            Interlocked.Exchange(ref snapshotTicks, unknownTicks);
        }

        private TaskMetaInformation[] CutMetas(TaskMetaInformation[] metas)
        {
            //NOTE hack code for tests
            var ticks = Interlocked.Read(ref minTicksHack);
            if(ticks <= 0)
                return metas;
            var list = new List<TaskMetaInformation>();
            foreach(var taskMetaInformation in metas)
            {
                if(taskMetaInformation.Ticks > ticks)
                    list.Add(taskMetaInformation);
            }
            return list.ToArray();
        }

        public void ProcessNewEvents()
        {
            lock(lockObject)
            {
                var nowTicks = globalTime.GetNowTicks();
                var fromTicks = Interlocked.Read(ref lastTicks);

                var indexingFinishTimestamp = indexingProgressMarkerStorage.IndexingFinishTimestamp;
                if(indexingFinishTimestamp != null && fromTicks > indexingFinishTimestamp.Ticks) //NOTE hack
                {
                    Log.For(this).LogInfoFormat(string.Format("IndexingFinishTimestamp is reached: {0}", indexingFinishTimestamp));
                    return;
                }

                if(fromTicks == unknownTicks)
                {
                    fromTicks = GetLastTicks();
                    Interlocked.Exchange(ref lastTicks, fromTicks);
                    Interlocked.Exchange(ref snapshotTicks, fromTicks);
                }

                Log.For(this).LogInfoFormat("Processing new events from {0} to {1}", DateTimeFormatter.FormatWithMsAndTicks(fromTicks), DateTimeFormatter.FormatWithMsAndTicks(nowTicks));

                var hasEvents = false;

                unprocessedEventsMap.CollectGarbage(nowTicks);
                //NOTE collectGarbage before unprocessedEventsMap.GetEvents() to kill trash events that has no meta
                var unprocessedEvents = unprocessedEventsMap.GetEvents();

                processedEventsMap.CollectGarbage(nowTicks);
                reader.CollectGarbage(nowTicks);
                var newEvents = GetEvents(fromTicks);

                unprocessedEvents.Concat(newEvents)
                                 .Batch(maxBatch, Enumerable.ToArray)
                                 .ForEach(events =>
                                     {
                                         hasEvents = true;
                                         ProcessEventsBatch(events, nowTicks);
                                     });

                if(!hasEvents)
                    ProcessEventsBatch(new TaskMetaUpdatedEvent[0], nowTicks);
            }
        }

        private long GetLastTicks()
        {
            Log.For(this).LogInfoFormat("Loading Last ticks");
            var lastReadTicks = indexingProgressMarkerStorage.GetLastReadTicks();
            Log.For(this).LogInfoFormat("Last ticks loaded. Value={0}", DateTimeFormatter.FormatWithMsAndTicks(lastReadTicks));
            return lastReadTicks;
        }

        private void ProcessEventsBatch(TaskMetaUpdatedEvent[] taskMetaUpdatedEvents, long now)
        {
            var indexingFinishTimestamp = indexingProgressMarkerStorage.IndexingFinishTimestamp;
            if(indexingFinishTimestamp != null && Interlocked.Read(ref lastTicks) > indexingFinishTimestamp.Ticks) //NOTE hack
                return;

            var actualMetas = graphiteReporter.ReportTiming("ReadTaskMetas", () => ReadActualMetas(taskMetaUpdatedEvents, now));
            actualMetas = CutMetas(actualMetas);

            taskMetaProcessor.IndexMetas(actualMetas);

            var ticks = GetSafeTimeForSnapshot(now, actualMetas);
            Interlocked.Exchange(ref snapshotTicks, ticks);
            SaveSnapshot(ticks);
            unprocessedEventsMap.CollectGarbage(now);
            processedEventsMap.CollectGarbage(now);
            reader.CollectGarbage(now);

            var lt = Interlocked.Read(ref lastTicks);
            if(lt < ticks)
                Interlocked.Exchange(ref lastTicks, ticks);
        }

        private TaskMetaInformation[] ReadActualMetas(TaskMetaUpdatedEvent[] taskMetaUpdatedEvents, long now)
        {
            var taskMetaInformations = reader.ReadActualMetasQuiet(taskMetaUpdatedEvents, now);
            var actualMetas = new List<TaskMetaInformation>();
            for(var i = 0; i < taskMetaInformations.Length; i++)
            {
                var taskMetaInformation = taskMetaInformations[i];
                var taskMetaUpdatedEvent = taskMetaUpdatedEvents[i];
                if(taskMetaInformation != null)
                {
                    actualMetas.Add(taskMetaInformation);
                    processedEventsMap.AddEvent(taskMetaUpdatedEvent);
                    unprocessedEventsMap.RemoveEvent(taskMetaUpdatedEvent);
                }
                else
                    unprocessedEventsMap.AddEvent(taskMetaUpdatedEvent);
            }

            var actualMetasArray = actualMetas.ToArray();
            return actualMetasArray;
        }

        public void SendActualizationLagToGraphite()
        {
            var lag = GetActualizationLag();
            if(lag.HasValue)
                graphiteReporter.ReportActualizationLag(lag.Value);
        }

        private TimeSpan? GetActualizationLag()
        {
            var ticks = Interlocked.Read(ref snapshotTicks);
            if(ticks != unknownTicks)
                return TimeSpan.FromTicks(DateTime.UtcNow.Ticks - ticks);
            return null;
        }

        public void SetMinTicksHack(long minTicks)
        {
            Interlocked.Exchange(ref minTicksHack, minTicks);
        }

        private long GetSafeTimeForSnapshot(long now, TaskMetaInformation[] taskMetaInformations)
        {
            var lastTicksEstimation = GetMinTicks(taskMetaInformations, now);
            var oldestEventTime = unprocessedEventsMap.GetOldestEventTime();
            if(oldestEventTime != null)
                lastTicksEstimation = Math.Min(lastTicksEstimation, Math.Max(oldestEventTime.Value, (DateTime.UtcNow - TimeSpan.FromMinutes(15)).Ticks));
            return lastTicksEstimation;
        }

        private void SaveSnapshot(long ticks)
        {
            if(ticks != unknownTicks)
            {
                Log.For(this).LogInfoFormat("Snapshot moved to {0}", DateTimeFormatter.FormatWithMsAndTicks(ticks));
                indexingProgressMarkerStorage.SetLastReadTicks(ticks);
            }
        }

        private static long GetMinTicks(TaskMetaInformation[] taskMetaInformations, long now)
        {
            if(taskMetaInformations.Length <= 0)
                return now;
            var minTicks = taskMetaInformations.Min(x => x.LastModificationTicks.Value);
            return minTicks;
        }

        private IEnumerable<TaskMetaUpdatedEvent> GetEvents(long fromTicks)
        {
            return eventLogRepository.GetEvents(fromTicks - unstableZoneTicks, maxBatch).Where(processedEventsMap.NotContains);
        }

        public void LogStatus()
        {
            Log.For(this).LogInfoFormat("Status: {0}", GetStatus().ToPrettyJson());
        }

        public ElasticMonitoringStatus GetStatus()
        {
            return new ElasticMonitoringStatus
                {
                    MinTicksHack = Interlocked.Read(ref minTicksHack),
                    UnprocessedMapLength = unprocessedEventsMap.GetUnsafeCount(),
                    ProcessedMapLength = processedEventsMap.GetUnsafeCount(),
                    ActualizationLag = GetActualizationLag(),
                    LastTicks = Interlocked.Read(ref lastTicks),
                    SnapshotTicks = Interlocked.Read(ref snapshotTicks),
                    NowTicks = DateTime.UtcNow.Ticks,
                    MetaCacheSize = reader.UnsafeGetCount(),
                };
        }

        private const int maxBatch = 1500;
        private const long unknownTicks = long.MinValue;
        private long minTicksHack;
        private long lastTicks;
        private long snapshotTicks;
        private readonly object lockObject = new object();
        private readonly long unstableZoneTicks;
        private readonly ITaskMetaProcessor taskMetaProcessor;
        private readonly IRtqElasticsearchIndexingProgressMarkerStorage indexingProgressMarkerStorage;
        private readonly IRtqElasticsearchIndexerGraphiteReporter graphiteReporter;
        private readonly IGlobalTime globalTime;
        private readonly IEventLogRepository eventLogRepository;
        private readonly IMetaCachedReader reader;
        private readonly EventsMap unprocessedEventsMap;
        private readonly EventsMap processedEventsMap;
    }
}