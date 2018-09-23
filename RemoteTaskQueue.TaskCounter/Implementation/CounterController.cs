using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using GroboContainer.Infection;

using log4net;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using RemoteTaskQueue.TaskCounter.Implementation.OldWaitingTasksCounters;
using RemoteTaskQueue.TaskCounter.Implementation.Utils;

using SkbKontur.Graphite.Client;

using SKBKontur.Catalogue.Core.Configuration.Settings;

namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public class CounterController : ICounterController
    {
        public CounterController(IEventLogRepository eventLogRepository,
                                 IMetaCachedReader reader,
                                 ICompositeCounter compositeCounter,
                                 IGlobalTime globalTime,
                                 ICounterControllerSnapshotStorage counterControllerSnapshotStorage,
                                 IGraphiteClient graphiteClient,
                                 long maxHistoryDepthTicks,
                                 int maxBatch,
                                 long snapshotSaveIntervalTicks,
                                 int maxSnapshotLength)
        {
            this.eventLogRepository = eventLogRepository;
            this.reader = reader;
            this.compositeCounter = compositeCounter;
            this.globalTime = globalTime;
            this.counterControllerSnapshotStorage = counterControllerSnapshotStorage;
            this.maxHistoryDepthTicks = maxHistoryDepthTicks;
            this.maxBatch = maxBatch;
            this.snapshotSaveIntervalTicks = snapshotSaveIntervalTicks;
            this.maxSnapshotLength = maxSnapshotLength;
            unstableZoneTicks = eventLogRepository.UnstableZoneLength.Ticks;
            unprocessedEventsMap = new UnprocessedEventsMap(unstableZoneTicks * 2);
            lastTicks = long.MinValue;
            GraphitePoster = new GraphitePoster(graphiteClient, compositeCounter);
        }

        [ContainerConstructor]
        public CounterController(RemoteQueue.Handling.RemoteTaskQueue remoteTaskQueue,
                                 MetaCachedReader reader,
                                 IApplicationSettings applicationSettings,
                                 IGraphiteClient graphiteClient)
            : this(remoteTaskQueue.EventLogRepository,
                   reader,
                   new CompositeCounter(new OldWaitingTasksCounter()),
                   remoteTaskQueue.GlobalTime,
                   new CounterControllerSnapshotStorage(remoteTaskQueue.Serializer, applicationSettings),
                   graphiteClient,
                   CounterSettings.MaxHistoryDepth.Ticks,
                   CounterSettings.MaxBatch,
                   CounterSettings.CounterSaveSnapshotInterval.Ticks,
                   CounterSettings.MaxStoredSnapshotLength)
        {
        }

        public GraphitePoster GraphitePoster { get; }

        private long GetNow()
        {
            return globalTime.GetNowTicks();
        }

        public void Restart(long? newStartTicks)
        {
            lock (lockObject)
            {
                InternalRestart(newStartTicks, GetNow());
            }
        }

        private void InternalRestart(long? newStartTicks, long now)
        {
            var startTicksCopy = newStartTicks ?? now - maxHistoryDepthTicks;
            logger.LogInfoFormat("Restart to {0}", DateTimeFormatter.FormatWithMsAndTicks(startTicksCopy));
            Interlocked.Exchange(ref startTicks, startTicksCopy);
            lastTicks = startTicksCopy;
            lastSnapshotSavedTime = lastTicks - snapshotSaveIntervalTicks;
            compositeCounter.Reset();
            unprocessedEventsMap.Clear();
        }

        public TaskCount GetTotalCount()
        {
            var totalCount = compositeCounter.GetTotalCount();
            totalCount.StartTicks = Interlocked.Read(ref startTicks);
            return totalCount;
        }

        public void ProcessNewEvents()
        {
            lock (lockObject)
            {
                var now = GetNow();

                if (lastTicks == long.MinValue)
                    LoadLastState(now);

                var hasEvents = false;

                var unprocessedEvents = unprocessedEventsMap.GetUnprocessedEvents(now);
                var newEvents = GetEvents(lastTicks, now);

                unprocessedEvents.Concat(newEvents)
                                 .Batch(maxBatch, Enumerable.ToArray)
                                 .ForEach(events =>
                                     {
                                         hasEvents = true;
                                         ProcessEventsBatch(events, now);
                                     });

                if (!hasEvents)
                    ProcessEventsBatch(new TaskMetaUpdatedEvent[0], now);

                lastTicks = now;
            }
        }

        private void LoadLastState(long now)
        {
            logger.LogInfoFormat("Loading saved state");
            var snapshot = counterControllerSnapshotStorage.ReadSnapshotOrNull();
            if (snapshot != null)
            {
                lastTicks = snapshot.ControllerTicks;
                lastSnapshotSavedTime = lastTicks - snapshotSaveIntervalTicks;
                compositeCounter.LoadSnapshot(snapshot.CounterSnapshot);
                Interlocked.Exchange(ref startTicks, lastTicks);
                logger.LogInfoFormat("State loaded. LastTicks={0}", DateTimeFormatter.FormatWithMsAndTicks(lastTicks));
            }
            else
            {
                logger.LogInfoFormat("Snapshot is null");
                InternalRestart(null, now);
            }
        }

        private void ProcessEventsBatch(TaskMetaUpdatedEvent[] taskMetaUpdatedEvents, long now)
        {
            var actualMetas = ReadActualMetas(taskMetaUpdatedEvents, now);
            compositeCounter.ProcessMetas(actualMetas, now);
            var ticks = GetSafeTimeForSnapshot(now, actualMetas);
            TrySaveSnapshot(ticks, now);
        }

        private TaskMetaInformation[] ReadActualMetas(TaskMetaUpdatedEvent[] taskMetaUpdatedEvents, long now)
        {
            var taskMetaInformations = reader.ReadActualMetasQuiet(taskMetaUpdatedEvents, now);
            var actualMetas = new List<TaskMetaInformation>();
            for (var i = 0; i < taskMetaInformations.Length; i++)
            {
                var taskMetaInformation = taskMetaInformations[i];
                if (taskMetaInformation != null)
                {
                    actualMetas.Add(taskMetaInformation);
                    unprocessedEventsMap.RemoveEvent(taskMetaUpdatedEvents[i]);
                }
                else
                    unprocessedEventsMap.AddEvent(taskMetaUpdatedEvents[i]);
            }

            var actualMetasArray = actualMetas.ToArray();
            return actualMetasArray;
        }

        private long GetSafeTimeForSnapshot(long now, TaskMetaInformation[] taskMetaInformations)
        {
            var lastTicksEstimation = GetMinTicks(taskMetaInformations, now);
            var oldestEventTime = unprocessedEventsMap.GetOldestEventTime();
            if (oldestEventTime != null)
                lastTicksEstimation = Math.Min(lastTicksEstimation, oldestEventTime.Value);
            return lastTicksEstimation;
        }

        private void TrySaveSnapshot(long ticks, long now)
        {
            if (now - lastSnapshotSavedTime < snapshotSaveIntervalTicks)
                return;
            var counterSnapshot = compositeCounter.GetSnapshotOrNull(maxSnapshotLength);
            if (counterSnapshot != null)
            {
                lastSnapshotSavedTime = now;
                counterControllerSnapshotStorage.SaveSnapshot(new CounterControllerSnapshot()
                    {
                        ControllerTicks = ticks,
                        CounterSnapshot = counterSnapshot
                    });
            }
            else
                logger.LogWarnFormat("Snapshot is too big");
        }

        private static long GetMinTicks(TaskMetaInformation[] taskMetaInformations, long now)
        {
            if (taskMetaInformations.Length <= 0)
                return now;
            var minTicks = taskMetaInformations.Min(x => x.LastModificationTicks.Value);
            return minTicks;
        }

        private IEnumerable<TaskMetaUpdatedEvent> GetEvents(long fromTicks, long toTicks)
        {
            //todo do not read event twice
            return eventLogRepository.GetEvents(fromTicks - unstableZoneTicks, toTicks, maxBatch);
        }

        private static readonly ILog logger = LogManager.GetLogger("CounterController");

        private long startTicks;
        private long lastTicks;
        private long lastSnapshotSavedTime;

        private readonly long maxHistoryDepthTicks;
        private readonly int maxBatch;
        private readonly long snapshotSaveIntervalTicks;
        private readonly int maxSnapshotLength;

        private readonly ICounterControllerSnapshotStorage counterControllerSnapshotStorage;
        private readonly IGlobalTime globalTime;
        private readonly IEventLogRepository eventLogRepository;
        private readonly IMetaCachedReader reader;
        private readonly UnprocessedEventsMap unprocessedEventsMap;

        private readonly object lockObject = new object();
        private readonly ICompositeCounter compositeCounter;
        private readonly long unstableZoneTicks;
    }
}