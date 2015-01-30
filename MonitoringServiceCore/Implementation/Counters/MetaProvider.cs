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

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters
{
    public class MetaProvider
    {
        [ContainerConstructor]
        public MetaProvider(IEventLogRepository eventLogRepository, IGlobalTime globalTime, IHandleTasksMetaStorage handleTasksMetaStorage, IMetaConsumer[] consumers)
            : this(CounterSettings.EventGarbageCollectionTimeout.Ticks,
                   CounterSettings.MaxBatch,
                   GetMaxHistoryDepthTicks()
                   , eventLogRepository, globalTime, handleTasksMetaStorage, consumers)
        {
        }

        public MetaProvider(long maxEventLifetimeTicks, int maxBatch, long startTicks, IEventLogRepository eventLogRepository, IGlobalTime globalTime, IHandleTasksMetaStorage handleTasksMetaStorage, IMetaConsumer[] consumers)
        {
            this.maxEventLifetimeTicks = maxEventLifetimeTicks;
            this.eventLogRepository = eventLogRepository;
            this.globalTime = globalTime;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.consumers = consumers;
            this.maxBatch = maxBatch;
            unstableZoneTicks = eventLogRepository.UnstableZoneLength.Ticks;
            cacheEventTicks = unstableZoneTicks * 2;
            InternalRestart(startTicks);
            Start();
        }

        public void Start()
        {
            logger.Info("MetaProvider Start");
            Interlocked.Increment(ref workingIfGreaterThanZero);
        }

        public void Stop()
        {
            var value = Interlocked.Decrement(ref workingIfGreaterThanZero);
            logger.InfoFormat("MetaProvider Stop. stopped = {0}", value <= 0);
        }

        public void FetchMetas()
        {
            if(!CanWork()) return;
            lock(updateLock)
                UpdateNoLock();
        }

        public void Restart(long? fromTicksUtc)
        {
            InternalRestart(fromTicksUtc);
        }

        public MetaProviderSnapshot GetSnapshotOrNull(int maxLength)
        {
            lock(dataLock)
            {
                if(notReadEvents.Count > maxLength || readEvents.Count > maxLength) //todo ?? different limits
                    return null;
                return new MetaProviderSnapshot(lastUpdateTicks, startTicks, notReadEvents, readEvents);
            }
        }

        public void LoadSnapshot(MetaProviderSnapshot snapshot, long notEarlierTicksUtc)
        {
            if(snapshot.LastUpdateTicks < notEarlierTicksUtc)
            {
                logger.WarnFormat("Snapshot time={0} is too old. Recalculating counter from {1}",
                                  DateTimeFormatter.FormatWithMsAndTicks(snapshot.LastUpdateTicks),
                                  DateTimeFormatter.FormatWithMsAndTicks(notEarlierTicksUtc));
                Restart(notEarlierTicksUtc);
            }
            else
            {
                logger.InfoFormat("Loading Snapshot. lastUpdate={0} startTime={1}. NotEarlier={2}",
                                  DateTimeFormatter.FormatWithMsAndTicks(snapshot.LastUpdateTicks),
                                  DateTimeFormatter.FormatWithMsAndTicks(snapshot.StartTicks),
                                  DateTimeFormatter.FormatWithMsAndTicks(notEarlierTicksUtc));
                lock(updateLock)
                    lock(dataLock)
                    {
                        lastUpdateTicks = snapshot.LastUpdateTicks;
                        startTicks = snapshot.StartTicks;
                        notReadEvents = snapshot.NotReadEvents ?? new Dictionary<string, long>();
                        readEvents = snapshot.ReadEvents ?? new Dictionary<string, long>();
                    }
            }
        }

        public static long GetMaxHistoryDepthTicks()
        {
            return (DateTime.UtcNow - CounterSettings.MaxHistoryDepth).Ticks;
        }

        public long StartTicks
        {
            get
            {
                lock(dataLock)
                    return startTicks;
            }
        }

        public class MetaProviderSnapshot
        {
            public MetaProviderSnapshot(long lastUpdateTicks, long startTicks, Dictionary<string, long> notReadEvents, Dictionary<string, long> readEvents)
            {
                if(notReadEvents != null) NotReadEvents = new Dictionary<string, long>(notReadEvents);
                if(readEvents != null) ReadEvents = new Dictionary<string, long>(readEvents);
                LastUpdateTicks = lastUpdateTicks;
                StartTicks = startTicks;
            }

            public long LastUpdateTicks { get; private set; }
            public long StartTicks { get; private set; }

            public Dictionary<string, long> NotReadEvents { get; private set; }
            public Dictionary<string, long> ReadEvents { get; private set; }
        }

        private bool CanWork()
        {
            return Interlocked.CompareExchange(ref workingIfGreaterThanZero, 0, 0) > 0;
        }

        private void InternalRestart(long? fromTicksUtc)
        {
            var ticks = fromTicksUtc == null ? GetMaxHistoryDepthTicks() : fromTicksUtc.Value;
            lock(dataLock)
            {
                logger.InfoFormat("Restarting Counter to time={0}", DateTimeFormatter.FormatWithMsAndTicks(ticks));
                startTicks = ticks;
                lastUpdateTicks = ticks;
                notReadEvents = new Dictionary<string, long>();
                readEvents = new Dictionary<string, long>();
            }
        }

        private void NotifyConsumers(TaskMetaInformation[] metas, long readTicks)
        {
            foreach(var metaConsumer in consumers)
                metaConsumer.NewMetainformationAvailable(metas, readTicks);
        }

        private void UpdateNoLock()
        {
            var nowTicks = globalTime.GetNowTicks();
            var events = eventLogRepository.GetEvents(GetLastTicks(), maxBatch);

            var notEmpty = false;
            events.Batch(maxBatch).ForEach(enumerable =>
                {
                    notEmpty = true;
                    ProcessEventsBatch(enumerable, nowTicks);
                });

            if(!notEmpty)
                ProcessEventsBatch(new TaskMetaUpdatedEvent[0], nowTicks);

            lock(dataLock)
                lastUpdateTicks = nowTicks;
        }

        private void CollectUnprocessedEventsGarbage(Dictionary<string, long> events, long nowTicks)
        {
            var idsToRemove = new HashSet<string>();
            foreach(var pair in events)
            {
                if(pair.Value + maxEventLifetimeTicks < nowTicks)
                    idsToRemove.Add(pair.Key);
            }
            DeleteByKeys(events, idsToRemove);
        }

        private void CollectAlreadyReadEventsGarbage(Dictionary<string, long> events, long nowTicks)
        {
            var idsToRemove = new HashSet<string>();
            foreach(var pair in events)
            {
                if(pair.Value + cacheEventTicks < nowTicks)
                    idsToRemove.Add(pair.Key);
            }
            DeleteByKeys(events, idsToRemove);
        }

        private static void DeleteByKeys(Dictionary<string, long> readEvents, IEnumerable<string> idsToRemove)
        {
            foreach(var id in idsToRemove)
                readEvents.Remove(id);
        }

        private void ProcessEventsBatch(IEnumerable<TaskMetaUpdatedEvent> events, long nowTicks)
        {
            Dictionary<string, long> readEventsCopy;
            Dictionary<string, long> notReadEventsCopy;
            lock(dataLock)
            {
                readEventsCopy = new Dictionary<string, long>(readEvents);
                notReadEventsCopy = new Dictionary<string, long>(notReadEvents);
            }

            CollectAlreadyReadEventsGarbage(readEventsCopy, nowTicks);
            CollectUnprocessedEventsGarbage(notReadEventsCopy, nowTicks);

            foreach(var @event in events)
            {
                var taskId = @event.TaskId;
                UpdateMaxTicks(notReadEventsCopy, taskId, @event.Ticks);
            }

            RemoveAlreadyReadEvents(notReadEventsCopy, readEventsCopy);

            var metas = handleTasksMetaStorage.GetMetas(notReadEventsCopy.Keys.ToArray());
            var newMetas = new List<TaskMetaInformation>();
            foreach(var meta in metas)
            {
                if(meta.LastModificationTicks.HasValue)
                {
                    if(notReadEventsCopy[meta.Id] <= meta.LastModificationTicks.Value)
                    {
                        readEventsCopy[meta.Id] = meta.LastModificationTicks.Value;
                        notReadEventsCopy.Remove(meta.Id);
                        newMetas.Add(meta);
                    }
                }
            }

            NotifyConsumers(newMetas.ToArray(), nowTicks);

            lock(dataLock)
            {
                notReadEvents = notReadEventsCopy;
                readEvents = readEventsCopy;
            }
        }

        private static void RemoveAlreadyReadEvents(Dictionary<string, long> notReadEventsCopy, Dictionary<string, long> readEventsCopy)
        {
            var taskIdsToRemove = new HashSet<string>();
            foreach(var kvp in notReadEventsCopy)
            {
                var taskId = kvp.Key;
                long lastReadTicks;
                if(readEventsCopy.TryGetValue(taskId, out lastReadTicks) && lastReadTicks >= kvp.Value)
                    taskIdsToRemove.Add(taskId);
                else
                    taskIdsToRemove.Remove(taskId);
            }

            DeleteByKeys(notReadEventsCopy, taskIdsToRemove);
        }

        private static void UpdateMaxTicks(Dictionary<string, long> taskIdToMaxTicks, string taskId, long currentTicks)
        {
            long oldTicks;
            if(!taskIdToMaxTicks.TryGetValue(taskId, out oldTicks))
                taskIdToMaxTicks.Add(taskId, currentTicks);
            else
            {
                if(currentTicks > oldTicks)
                    taskIdToMaxTicks[taskId] = currentTicks;
            }
        }

        private long GetLastTicks()
        {
            long lastUpdateTicksCopy;
            lock(dataLock)
                lastUpdateTicksCopy = lastUpdateTicks;
            var lastTicks = lastUpdateTicksCopy - unstableZoneTicks;
            if(lastTicks < 0)
                return 0;
            return lastTicks;
        }

        private int workingIfGreaterThanZero;

        private volatile Dictionary<string, long> notReadEvents;
        private volatile Dictionary<string, long> readEvents;

        private long startTicks;
        private long lastUpdateTicks;

        private static readonly ILog logger = LogManager.GetLogger("MetaProvider");

        private readonly IMetaConsumer[] consumers;

        private readonly IEventLogRepository eventLogRepository;
        private readonly IGlobalTime globalTime;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;

        private readonly object updateLock = new object();
        private readonly object dataLock = new object();

        private readonly int maxBatch;
        private readonly long maxEventLifetimeTicks;
        private readonly long cacheEventTicks;
        private readonly long unstableZoneTicks;
    }
}